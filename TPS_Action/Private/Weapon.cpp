#include "Weapon.h"
#include "Components/BoxComponent.h"
#include "Components/SphereComponent.h"
#include "ActionCharacter.h"
#include "Kismet/KismetSystemLibrary.h"
#include "HitInterface.h"

AWeapon::AWeapon()
{
 	PrimaryActorTick.bCanEverTick = true;

	WeaponMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("WeaponMeshComponent"));
	RootComponent = WeaponMesh;

	Sphere = CreateDefaultSubobject<USphereComponent>(TEXT("Sphere"));
	Sphere->SetupAttachment(GetRootComponent());
	Sphere->SetCollisionEnabled(ECollisionEnabled::QueryOnly);

	Box = CreateDefaultSubobject<UBoxComponent>(TEXT("Box"));
	Box->SetupAttachment(GetRootComponent());
	Box->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	Box->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Overlap);
	Box->SetCollisionResponseToChannel(ECollisionChannel::ECC_Pawn, ECollisionResponse::ECR_Ignore);

	BoxTraceStart = CreateDefaultSubobject<USceneComponent>(TEXT("Box Trace Start"));
	BoxTraceStart->SetupAttachment(GetRootComponent());
	BoxTraceEnd= CreateDefaultSubobject<USceneComponent>(TEXT("Box Trace End"));
	BoxTraceEnd->SetupAttachment(GetRootComponent());
}

void AWeapon::BeginPlay()
{
	Super::BeginPlay();
	
	Sphere->OnComponentBeginOverlap.AddDynamic(this, &ThisClass::OnSphereOverlap);
	Box->OnComponentBeginOverlap.AddDynamic(this, &ThisClass::OnBoxOverlap);
}

void AWeapon::OnSphereOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	AActionCharacter* ActionCharacter = Cast<AActionCharacter>(OtherActor);
	if (ActionCharacter)
	{
		Owner = ActionCharacter;
		ActionCharacter->SetWeapon(this);
		AttachToSocket(ActionCharacter->GetMesh(), ActionCharacter->WeaponShoulderSocketName);
		Sphere->OnComponentBeginOverlap.RemoveAll(this);
	}
}

void AWeapon::OnBoxOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (OtherActor == Owner)
	{
		return;
	}

	const FVector Start = BoxTraceStart->GetComponentLocation();
	const FVector End = BoxTraceEnd->GetComponentLocation();
	
	TArray<FHitResult> BoxHits;
	bool IsHit = UKismetSystemLibrary::BoxTraceMultiByProfile(
		this, Start, End, FVector(7, 7, 7), BoxTraceStart->GetComponentRotation(),
		FName("Pawn"), false, ActorsToIgnore, EDrawDebugTrace::ForDuration,
		BoxHits, true
	);

	if (IsHit == false)
	{
		return;
	}

	for (int i = 0; i < BoxHits.Num(); i++)
	{
		FHitResult BoxHit = BoxHits[i];
		if (BoxHit.GetActor() == nullptr)
		{
			continue;
		}

		IHitInterface* HitInterface = Cast<IHitInterface>(BoxHit.GetActor());
		if (HitInterface)
		{
			HitInterface->GetHit(BoxHit.ImpactPoint);
			ActorsToIgnore.AddUnique(BoxHit.GetActor());
		}
	}
}

void AWeapon::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void AWeapon::AttachToSocket(USceneComponent* Parent, const FName socketName)
{
	FAttachmentTransformRules TransformRules(EAttachmentRule::SnapToTarget, true);
	WeaponMesh->AttachToComponent(Parent, TransformRules, socketName);
}

void AWeapon::EnableCollision()
{
	ActorsToIgnore.Empty();
	Box->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
}

void AWeapon::DisableCollision()
{
	Box->SetCollisionEnabled(ECollisionEnabled::NoCollision);
}

