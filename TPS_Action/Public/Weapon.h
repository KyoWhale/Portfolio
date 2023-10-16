#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Weapon.generated.h"

class UBoxComponent;
class USphereComponent;

enum class EWeaponState : uint8
{
	EWS_Unarmed UMETA(DisplayName = "Unarmed"),
	EWS_Armed UMETA(DisplayName = "Armed")
};

UCLASS()
class TPS_ACTION_API AWeapon : public AActor
{
	GENERATED_BODY()
	
public:	
	AWeapon();
	virtual void Tick(float DeltaTime) override;

	void AttachToSocket(USceneComponent* Parent, const FName socketName);
	void EnableCollision();
	void DisableCollision();

protected:
	virtual void BeginPlay() override;

	UFUNCTION() void OnSphereOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UFUNCTION() void OnBoxOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UPROPERTY(VisibleAnywhere, Category = Weapon) UStaticMeshComponent* WeaponMesh;
	EWeaponState WeaponState = EWeaponState::EWS_Unarmed;

private:
	class AActionCharacter* Owner;

	UPROPERTY(VisibleAnywhere) USphereComponent* Sphere;
	UPROPERTY(VisibleAnywhere) UBoxComponent* Box;

	UPROPERTY(VisibleAnywhere) USceneComponent* BoxTraceStart;
	UPROPERTY(VisibleAnywhere) USceneComponent* BoxTraceEnd;
	TArray<AActor*> ActorsToIgnore;

public:
	void SetWeaponState(EWeaponState NewState) { WeaponState = NewState; }
	EWeaponState GetWeaponState() { return WeaponState; }

};
