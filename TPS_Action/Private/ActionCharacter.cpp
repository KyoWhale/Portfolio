#include "ActionCharacter.h"
#include "Components/InputComponent.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Animation/AnimMontage.h"
#include "Weapon.h"
#include "DrawDebugHelpers.h"

AActionCharacter::AActionCharacter()
{
	PrimaryActorTick.bCanEverTick = true;

	bUseControllerRotationPitch = false;
	bUseControllerRotationRoll = false;
	bUseControllerRotationYaw = false;

	GetCharacterMovement()->bOrientRotationToMovement = true;
	GetCharacterMovement()->RotationRate = FRotator(0.f, 400.f, 0.f);

	CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
	CameraBoom->SetupAttachment(GetRootComponent());
	CameraBoom->TargetArmLength = 300.f;

	ViewCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("ViewCamera"));
	ViewCamera->SetupAttachment(CameraBoom);
}

void AActionCharacter::BeginPlay()
{
	Super::BeginPlay();
	
	if (APlayerController* PlayerController = Cast<APlayerController>(Controller))
	{
		if (UEnhancedInputLocalPlayerSubsystem* Subsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(PlayerController->GetLocalPlayer()))
		{
			Subsystem->AddMappingContext(AC_InputMappingContext, 0);
		}
	}
}

void AActionCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	//Super::SetupPlayerInputComponent(PlayerInputComponent);

	if (UEnhancedInputComponent* EnhancedInputComponent = CastChecked<UEnhancedInputComponent>(PlayerInputComponent))
	{
		EnhancedInputComponent->BindAction(AC_MoveAction, ETriggerEvent::Triggered, this, &AActionCharacter::Move);
		EnhancedInputComponent->BindAction(AC_LookAction, ETriggerEvent::Triggered, this, &AActionCharacter::Look);

		EnhancedInputComponent->BindAction(AC_JumpAction, ETriggerEvent::Triggered, this, &AActionCharacter::Jump);
		EnhancedInputComponent->BindAction(AC_JumpAction, ETriggerEvent::Completed, this, &AActionCharacter::StopJumping);

		EnhancedInputComponent->BindAction(AC_AttackAction, ETriggerEvent::Triggered, this, &AActionCharacter::Attack);

		EnhancedInputComponent->BindAction(AC_GuardAction, ETriggerEvent::Triggered, this, &AActionCharacter::Guard);
		EnhancedInputComponent->BindAction(AC_GuardAction, ETriggerEvent::Completed, this, &AActionCharacter::StopGuard);

		EnhancedInputComponent->BindAction(AC_DodgeAction, ETriggerEvent::Triggered, this, &AActionCharacter::Dodge);
	}
}

void AActionCharacter::Move(const FInputActionValue& Value)
{
	LastInputVector = Value.Get<FVector2D>();
	const FRotator YawRotation(0.f, Controller->GetControlRotation().Yaw, 0.f);

	const FVector ControllerForward = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
	const FVector ControllerRight = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);
	FVector InputVector = (ControllerForward * LastInputVector.Y + ControllerRight * LastInputVector.X);
	InputVector.Normalize();
	DrawDebugLine(GetWorld(), GetActorLocation(), GetActorLocation()+InputVector*100, FColor::Blue, false, 1.5f, 0, 1);
	
	if (TryTurn(InputVector) == false)
	{
		AddMovementInput(InputVector);
	}
}

void AActionCharacter::Look(const FInputActionValue& Value)
{
	const FVector2D LookAxisVector = Value.Get<FVector2D>();

	AddControllerPitchInput(LookAxisVector.Y);
	AddControllerYawInput(LookAxisVector.X);
}

bool AActionCharacter::TryTurn(const FVector& InputVector)
{
	const FVector ForwardVector = GetActorForwardVector();
	const float CosTheta = ForwardVector.Dot(InputVector);

	if (CosTheta > 0.9f)
	{
		TurnState = ETurnState::ETS_Idle;
		return false;
	}

	const FVector CrossVector = FVector::CrossProduct(ForwardVector, InputVector);
	if (CosTheta < -0.8f)
	{
		TurnState = CrossVector.Z > 0 ? ETurnState::ETS_BackRight : ETurnState::ETS_BackLeft;
	}
	else if (CosTheta < -0.1f)
	{
		TurnState = CrossVector.Z > 0 ? ETurnState::ETS_Right : ETurnState::ETS_Left;
	}

	return true;
}

void AActionCharacter::Attack()
{
	switch (CharacterState)
	{
	case ECharacterState::ECS_Armed:
		if (ArmedState == EArmedState::EAS_Occupied)
		{
			return;
		}

		if (ArmedState == EArmedState::EAS_Attacking)
		{

		}

		PlayAttackMontage();
		break;
	case ECharacterState::ECS_Unarmed:
		if (CanArm() == false)
		{
			return;
		}

		PlayArmMontage();
		break;
	default:
		break;
	}
}

void AActionCharacter::Guard()
{
}

void AActionCharacter::StopGuard()
{
}

void AActionCharacter::Dodge()
{
}

bool AActionCharacter::CanDisarm()
{
	return CharacterState == ECharacterState::ECS_Armed &&
		ArmedState == EArmedState::EAS_Idle;
}

bool AActionCharacter::CanArm()
{
	return Weapon &&
		CharacterState == ECharacterState::ECS_Unarmed &&
		UnarmedState == EUnarmedState::EUS_Idle;
}

void AActionCharacter::PlayArmMontage(bool Equip)
{
	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && ArmMontage && AnimInstance->Montage_IsPlaying(ArmMontage) == false)
	{
		AnimInstance->Montage_Play(ArmMontage);
		AnimInstance->Montage_JumpToSection(FName(Equip ? "Arm" : "Unarm"));
	}
}

void AActionCharacter::PlayAttackMontage()
{
	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	static int8 sectionNumber = 1;
	if (AllowNextAttack && AnimInstance && AttackCombo1Montage)
	{
		if (AnimInstance->Montage_IsPlaying(AttackCombo1Montage) == false)
		{
			AnimInstance->Montage_Play(AttackCombo1Montage);
			sectionNumber = 1;
			ArmedState = EArmedState::EAS_Attacking;
			AllowNextAttack = false;
		}
		else if (sectionNumber < AttackCombo1Montage->GetNumSections())
		{
			AnimInstance->Montage_JumpToSection(*FString::Printf(TEXT("Attack%d"), ++sectionNumber));
			ArmedState = EArmedState::EAS_Attacking;
			AllowNextAttack = false;
		}
	}
}

void AActionCharacter::Arm()
{
	if (Weapon == nullptr)
	{
		return;
	}

	SetCharacterState(ECharacterState::ECS_Armed);
	SetArmedState(EArmedState::EAS_Idle);
	Weapon->AttachToSocket(GetMesh(), WeaponHandSocketName);
}

void AActionCharacter::Disarm()
{
	if (Weapon == nullptr)
	{
		return;
	}

	SetCharacterState(ECharacterState::ECS_Unarmed);
	SetUnarmedState(EUnarmedState::EUS_Idle);
	Weapon->AttachToSocket(GetMesh(), WeaponShoulderSocketName);
}

void AActionCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	DrawDebugLine(GetWorld(), GetActorLocation(), GetActorLocation() + GetActorForwardVector() * 100, FColor::Green, false, 1.5f, 0, 1);
}

