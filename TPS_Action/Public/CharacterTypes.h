#pragma once

UENUM(BlueprintType)
enum class ECharacterState : uint8
{
	ECS_Unarmed UMETA(DisplayName = "Unarmed"),
	ECS_Armed UMETA(DisplayName = "Armed")
};

UENUM(BlueprintType)
enum class ETurnState : uint8
{
	ETS_Idle UMETA(DisplayName = "Idle"),
	ETS_Left UMETA(DisplayName = "Left"),
	ETS_Right UMETA(DisplayName = "Right"),
	ETS_BackLeft UMETA(DisplayName = "Back Left"),
	ETS_BackRight UMETA(DisplayName = "Back Right")
};

UENUM(BlueprintType)
enum class EUnarmedState : uint8
{
	EUS_Idle UMETA(DisplayName = "Idle"),
	EUS_Occupied UMETA(DisplayName = "Occupied")
};

UENUM(BlueprintType)
enum class EArmedState : uint8
{
	EAS_Idle UMETA(DisplayName = "Idle"),
	EAS_Attacking UMETA(DisplayName = "Attacking"),
	EAS_Occupied UMETA(DisplayName = "Occupied")
};