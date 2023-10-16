// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "InputAction.h"
#include "CharacterTypes.h"
#include "Weapon.h"
#include "ActionCharacter.generated.h"

class UInputAction;
class UInputMappingContext;
class UAnimMontage;
class USpringArmComponent;
class UCameraComponent;

UCLASS()
class TPS_ACTION_API AActionCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	AActionCharacter();
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;
	virtual void Tick(float DeltaTime) override;

	void Arm();
	void Disarm();

protected:
	virtual void BeginPlay() override;

	void Move(const FInputActionValue& Value);
	void Look(const FInputActionValue& Value);
	bool TryTurn(const FVector& InputVector);
	void Attack();
	void Guard();
	void StopGuard();
	void Dodge();

	bool CanDisarm();
	bool CanArm();

	void PlayArmMontage(bool Equip = true);
	void PlayAttackMontage();

	UPROPERTY(EditAnywhere, Category = Input) UInputMappingContext* AC_InputMappingContext;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_MoveAction;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_LookAction;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_JumpAction;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_AttackAction;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_GuardAction;
	UPROPERTY(EditAnywhere, Category = Input) UInputAction* AC_DodgeAction;

	FVector2D LastInputVector;

private:
	UPROPERTY(VisibleAnywhere) USpringArmComponent* CameraBoom;
	UPROPERTY(VisibleAnywhere) UCameraComponent* ViewCamera;
	UPROPERTY(VisibleAnywhere) AWeapon* Weapon;

	UPROPERTY(EditDefaultsOnly, Category = Montages) UAnimMontage* TurnMontage;
	UPROPERTY(EditDefaultsOnly, Category = Montages) UAnimMontage* ArmMontage;
	UPROPERTY(EditDefaultsOnly, Category = Montages) UAnimMontage* AttackCombo1Montage;
	ECharacterState CharacterState = ECharacterState::ECS_Unarmed;
	EUnarmedState UnarmedState = EUnarmedState::EUS_Idle;
	EArmedState ArmedState = EArmedState::EAS_Idle;
	ETurnState TurnState = ETurnState::ETS_Idle;
	bool AllowNextAttack = true;

public:
	FORCEINLINE ECharacterState GetCharacterState() const { return CharacterState; }
	FORCEINLINE EUnarmedState GetUnequippedState() const { return UnarmedState; }
	FORCEINLINE EArmedState GetEquippedState() const { return ArmedState; }
	FORCEINLINE ETurnState GetTurnState() const { return TurnState; }

	FORCEINLINE void SetCharacterState(ECharacterState newState) { CharacterState = newState; }
	FORCEINLINE void SetUnarmedState(EUnarmedState newState) { UnarmedState = newState; }
	FORCEINLINE void SetArmedState(EArmedState newState) { ArmedState = newState; }

	FORCEINLINE void SetWeapon(AWeapon* NewWeapon) { this->Weapon = NewWeapon; }
	FORCEINLINE AWeapon* GetWeapon() { return Weapon; }
	FORCEINLINE void SetAllowNextAttack(bool Value) { AllowNextAttack = Value; }

	const FName WeaponHandSocketName = FName("weapon_sword");
	const FName WeaponShoulderSocketName = FName("sword_holder");
};
