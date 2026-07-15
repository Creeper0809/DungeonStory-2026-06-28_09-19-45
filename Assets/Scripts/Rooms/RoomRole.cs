using System;

[Flags]
public enum RoomRole
{
    None = 0,
    Dining = 1 << 0,
    Shop = 1 << 1,
    Rest = 1 << 2,
    Training = 1 << 3,
    Research = 1 << 4,
    Mana = 1 << 5,
    Storage = 1 << 6,
    Toilet = 1 << 7,
    Hygiene = 1 << 8,
    Administration = 1 << 9,
    Security = 1 << 10
}

public static class RoomRoleUtility
{
    public static RoomRole FromFacilityRoles(FacilityRole roles)
    {
        RoomRole result = RoomRole.None;
        if ((roles & FacilityRole.Meal) != 0) result |= RoomRole.Dining;
        if ((roles & FacilityRole.Purchase) != 0) result |= RoomRole.Shop;
        if ((roles & FacilityRole.Rest) != 0) result |= RoomRole.Rest;
        if ((roles & FacilityRole.Training) != 0) result |= RoomRole.Training;
        if ((roles & FacilityRole.Research) != 0) result |= RoomRole.Research;
        if ((roles & FacilityRole.Mana) != 0) result |= RoomRole.Mana;
        if ((roles & FacilityRole.Logistics) != 0) result |= RoomRole.Storage;
        if ((roles & FacilityRole.Toilet) != 0) result |= RoomRole.Toilet;
        if ((roles & FacilityRole.Hygiene) != 0) result |= RoomRole.Hygiene;
        if ((roles & FacilityRole.Administration) != 0) result |= RoomRole.Administration;
        if ((roles & FacilityRole.Security) != 0) result |= RoomRole.Security;
        return result;
    }

    public static FacilityRole ToFacilityRoles(RoomRole roles)
    {
        FacilityRole result = FacilityRole.None;
        if ((roles & RoomRole.Dining) != 0) result |= FacilityRole.Meal;
        if ((roles & RoomRole.Shop) != 0) result |= FacilityRole.Purchase;
        if ((roles & RoomRole.Rest) != 0) result |= FacilityRole.Rest;
        if ((roles & RoomRole.Training) != 0) result |= FacilityRole.Training;
        if ((roles & RoomRole.Research) != 0) result |= FacilityRole.Research;
        if ((roles & RoomRole.Mana) != 0) result |= FacilityRole.Mana;
        if ((roles & RoomRole.Storage) != 0) result |= FacilityRole.Logistics;
        if ((roles & RoomRole.Toilet) != 0) result |= FacilityRole.Toilet;
        if ((roles & RoomRole.Hygiene) != 0) result |= FacilityRole.Hygiene;
        if ((roles & RoomRole.Administration) != 0) result |= FacilityRole.Administration;
        if ((roles & RoomRole.Security) != 0) result |= FacilityRole.Security;
        return result;
    }
}
