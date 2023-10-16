// Fill out your copyright notice in the Description page of Project Settings.


#include "ActionCharacterAnimInstance.h"
#include "ActionCharacter.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "Weapon.h"

void UActionCharacterAnimInstance::NativeInitializeAnimation()
{
	Super::NativeInitializeAnimation();

	ActionCharacter = Cast<AActionCharacter>(TryGetPawnOwner());
	if (ActionCharacter)
	{
		ActionCharacterMovement = ActionCharacter->GetCharacterMovement();
	}
}

void UActionCharacterAnimInstance::NativeUpdateAnimation(float DeltaTime)
{
	Super::NativeUpdateAnimation(DeltaTime);

	if (ActionCharacterMovement)
	{
		GroundSpeed = UKismetMathLibrary::VSizeXY(ActionCharacterMovement->Velocity);
		IsFalling = ActionCharacterMovement->IsFalling();
		CharacterState = ActionCharacter->GetCharacterState();
		TurnState = ActionCharacter->GetTurnState();

		const double AccelerationSize = UKismetMathLibrary::VSizeXY(ActionCharacterMovement->GetCurrentAcceleration());
		HasAcceleration = FMath::IsNearlyZero(AccelerationSize) == false;

		const FVector CurrentLocation = ActionCharacter->GetActorLocation();
		LocationDiffSize = UKismetMathLibrary::VSizeXY(CurrentLocation - PreviousLocation);
		DeltaSpeed = UKismetMathLibrary::SafeDivide(LocationDiffSize, DeltaTime);
		PreviousLocation = CurrentLocation;
	}
}

void UActionCharacterAnimInstance::AnimNotify_AttackEnd()
{
	if (ActionCharacter)
	{
		ActionCharacter->SetArmedState(EArmedState::EAS_Idle);
		ActionCharacter->SetAllowNextAttack(true);
	}
}

void UActionCharacterAnimInstance::AnimNotify_Grab()
{
	if (ActionCharacter)
	{
		ActionCharacter->Arm();
	}
}

void UActionCharacterAnimInstance::AnimNotify_Release()
{
	if (ActionCharacter)
	{
		ActionCharacter->Disarm();
	}
}

void UActionCharacterAnimInstance::AnimNotify_AllowNextAttack()
{
	if (ActionCharacter)
	{
		ActionCharacter->SetAllowNextAttack(true);
	}
}

void UActionCharacterAnimInstance::AnimNotify_EnableWeaponCollision()
{
	if (ActionCharacter && ActionCharacter->GetWeapon())
	{
		ActionCharacter->GetWeapon()->EnableCollision();
	}
}

void UActionCharacterAnimInstance::AnimNotify_DisableWeaponCollision()
{
	if (ActionCharacter && ActionCharacter->GetWeapon())
	{
		ActionCharacter->GetWeapon()->DisableCollision();
	}
}

