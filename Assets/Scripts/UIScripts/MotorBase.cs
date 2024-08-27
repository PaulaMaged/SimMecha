using System.Collections.Generic;

public abstract class MotorBase
{
    public abstract string MotorName { get; }
    public abstract Dictionary<string, object> GetAttributes();
}

public class DC_Motor : MotorBase
{
    public float Voltage { get; set; }
    public override string MotorName => "DC_Motor";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "Voltage", Voltage }
        };
    }
}

public class StepperMotor : MotorBase
{
    public int MaxSpeed { get; set; }
    public override string MotorName => "StepperMotor";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "MaxSpeed", MaxSpeed }
        };
    }
}

public class DoublyFedInduction : MotorBase
{
    public float r_s { get; set; }  // Stator resistance (Ohm)
    public float r_r { get; set; }  // Rotor resistance (Ohm)
    public float l_m { get; set; }  // Main inductance (Henry)
    public float l_sigs { get; set; }  // Stator-side stray inductance (Henry)
    public float l_sigr { get; set; }  // Rotor-side stray inductance (Henry)
    public int p { get; set; }  // Pole pair number
    public float j_rotor { get; set; }  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "DoublyFedInduction";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_s", r_s },
            { "r_r", r_r },
            { "l_m", l_m },
            { "l_sigs", l_sigs },
            { "l_sigr", l_sigr },
            { "p", p },
            { "j_rotor", j_rotor }
        };
    }
}

public class ExtExcitedDc : MotorBase
{
    public float r_a { get; set; }  // Armature circuit resistance (Ohm)
    public float r_e { get; set; }  // Exciting circuit resistance (Ohm)
    public float l_a { get; set; }  // Armature circuit inductance (Henry)
    public float l_e { get; set; }  // Exciting circuit inductance (Henry)
    public float l_e_prime { get; set; }  // Effective excitation inductance (Henry)
    public float j_rotor { get; set; }  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "ExtExcitedDc";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_a", r_a },
            { "r_e", r_e },
            { "l_a", l_a },
            { "l_e", l_e },
            { "l_e_prime", l_e_prime },
            { "j_rotor", j_rotor }
        };
    }
}

public class ExtExcitedSynch : MotorBase
{
    public float r_s { get; set; }  // Stator resistance (mOhm)
    public float r_e { get; set; }  // Excitation resistance (mOhm)
    public float l_d { get; set; }  // Direct axis inductance (mH)
    public float l_q { get; set; }  // Quadrature axis inductance (mH)
    public float l_m { get; set; }  // Mutual inductance (mH)
    public float l_e { get; set; }  // Excitation inductance (mH)
    public int p { get; set; }  // Pole pair number
    public float j_rotor { get; set; }  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "ExtExcitedSynch";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_s", r_s },
            { "r_e", r_e },
            { "l_d", l_d },
            { "l_q", l_q },
            { "l_m", l_m },
            { "l_e", l_e },
            { "p", p },
            { "j_rotor", j_rotor }
        };
    }
}

public class PermExcitedDc : MotorBase
{
    public float r_a { get; set; }  // Armature circuit resistance (Ohm)
    public float l_a { get; set; }  // Armature circuit inductance (Henry)
    public float j_rotor { get; set; }  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "PermExcitedDc";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_a", r_a },
            { "l_a", l_a },
            { "j_rotor", j_rotor }
        };
    }
}



