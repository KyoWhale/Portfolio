// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "HitInterface.h"
#include "Enemy.generated.h"

class UAnimMontage;

UCLASS()
class TPS_ACTION_API AEnemy : public ACharacter, public IHitInterface
{
	GENERATED_BODY()

public:
	AEnemy();
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;
	virtual void Tick(float DeltaTime) override;

	virtual void GetHit(const FVector& ImpactPoint) override;

protected:
	virtual void BeginPlay() override;

	void PlayHitMontage(const FName& PositionName);

private:
	UPROPERTY(EditDefaultsOnly, Category = Montages) UAnimMontage* HitMontage;

public:	

};
