#include "Enemy.h"
#include "Components/SkeletalMeshComponent.h"
#include "Components/CapsuleComponent.h"
#include "Animation/AnimMontage.h"

AEnemy::AEnemy()
{
	PrimaryActorTick.bCanEverTick = true;

	GetMesh()->SetGenerateOverlapEvents(true);
	//GetMesh()->SetCollisionObjectType(ECollisionChannel::ECC_WorldDynamic);
	GetMesh()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Visibility, ECollisionResponse::ECR_Block);
	GetMesh()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);
	GetCapsuleComponent()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);
}

void AEnemy::BeginPlay()
{
	Super::BeginPlay();
	
}

void AEnemy::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);
}

void AEnemy::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

void AEnemy::GetHit(const FVector& ImpactPoint)
{
	const FVector Forward = GetActorForwardVector();
	const FVector ToHit = (FVector(ImpactPoint.X, ImpactPoint.Y, GetActorLocation().Z) - GetActorLocation()).GetSafeNormal();
	
	const FVector CrossProduct = FVector::CrossProduct(Forward, ToHit);
	const double CosTheta = FVector::DotProduct(Forward, ToHit);
	const double Theta = FMath::RadiansToDegrees(FMath::Acos(CosTheta));

	FName Section("Back");
	if (Theta <= 45.f)
	{
		Section = FName("Front");
	}
	else if (Theta <= 135.f)
	{
		if (CrossProduct.Z > 0)
		{
			Section = FName("Right");
		}
		else
		{
			Section = FName("Left");
		}
	}

	PlayHitMontage(Section);
}

void AEnemy::PlayHitMontage(const FName& PositionName)
{
	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && HitMontage)
	{
		AnimInstance->Montage_Play(HitMontage);
		AnimInstance->Montage_JumpToSection(PositionName);
	}
}

