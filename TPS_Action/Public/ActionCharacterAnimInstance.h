// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Animation/AnimInstance.h"
#include "CharacterTypes.h"
#include "ActionCharacterAnimInstance.generated.h"

class AActionCharacter;
class UCharacterMovementComponent;

UCLASS()
class TPS_ACTION_API UActionCharacterAnimInstance : public UAnimInstance
{
	GENERATED_BODY()

public:
	virtual void NativeInitializeAnimation() override;
	virtual void NativeUpdateAnimation(float DeltaTime) override;
	
	UFUNCTION() void AnimNotify_AttackEnd();
	UFUNCTION() void AnimNotify_Grab();
	UFUNCTION() void AnimNotify_Release();
	UFUNCTION() void AnimNotify_AllowNextAttack();
	UFUNCTION() void AnimNotify_EnableWeaponCollision();
	UFUNCTION() void AnimNotify_DisableWeaponCollision();
	
	UPROPERTY(BlueprintReadOnly) AActionCharacter* ActionCharacter;
	UPROPERTY(BlueprintReadOnly, Category = Movement) ECharacterState CharacterState = ECharacterState::ECS_Unarmed;
	UPROPERTY(BlueprintReadOnly, Category = Movement) UCharacterMovementComponent* ActionCharacterMovement;

	UPROPERTY(BlueprintReadOnly, Category = Movement) float GroundSpeed;
	UPROPERTY(BlueprintReadOnly, Category = Movement) float DeltaSpeed;
	UPROPERTY(BlueprintReadOnly, Category = Movement) float LocationDiffSize;
	UPROPERTY(BlueprintReadOnly, Category = Movement) bool IsFalling;
	UPROPERTY(BlueprintReadOnly, Category = Movement) bool HasAcceleration;
	UPROPERTY(BlueprintReadOnly, Category = Movement) ETurnState TurnState;

private:
	FVector PreviousLocation;
};
