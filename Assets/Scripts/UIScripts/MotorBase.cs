using System.Collections.Generic;

public abstract class MotorBase
{
    public abstract string MotorName { get; }
    public abstract Dictionary<string, object> GetAttributes();
}

public class DoublyFedInduction : MotorBase
{
    public object r_s { get; set; } = 12.42;  // Stator resistance (Ohm)
    public object r_r { get; set; } = 3.51;  // Rotor resistance (Ohm)
    public object l_m { get; set; } = 297.5e-3;  // Main inductance (Henry)
    public object l_sigs { get; set; } = 25.71e-3;  // Stator-side stray inductance (Henry)
    public object l_sigr { get; set; } = 25.71e-3;  // Rotor-side stray inductance (Henry)
    public int p { get; set; } = 2;  // Pole pair number
    public object j_rotor { get; set; } = 13.695e-4;  // Moment of inertia of the rotor (kg·m²)

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

public class SquirrelCageInduction : MotorBase
{
    public object r_s { get; set; } = 66.9338;       // Stator resistance (Ohm)
    public object r_r { get; set; } = 55.355;        // Rotor resistance (Ohm)
    public object l_m { get; set; } = 143.75e-2;     // Main inductance (Henry)
    public object l_sigs { get; set; } = 5.87e-5;    // Stator-side stray inductance (Henry)
    public object l_sigr { get; set; } = 5.87e-3;    // Rotor-side stray inductance (Henry)
    public int p { get; set; } = 2;                  // Pole pair number
    public object j_rotor { get; set; } = 0.0011;    // Moment of inertia of the rotor (kg·m²)


    public override string MotorName => "SquirrelCageInduction";

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
    public object r_a { get; set; } = 1;  // Armature circuit resistance (Ohm)
    public object r_e { get; set; } = 1; // Exciting circuit resistance (Ohm)
    public object l_a { get; set; } = 19e-6;  // Armature circuit inductance (Henry)
    public object l_e { get; set; } = 5.4e-3;  // Exciting circuit inductance (Henry)
    public object l_e_prime { get; set; } = 1.7e-3; // Effective excitation inductance (Henry)
    public object j_rotor { get; set; } = 0.025;  // Moment of inertia of the rotor (kg·m²)

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
    public object r_s { get; set; } = 15.55e-1;  // Stator resistance (mOhm)
    public object r_e { get; set; } = 7.2e-1;    // Excitation resistance (mOhm)
    public object l_d { get; set; } = 1.66e-1;   // Direct axis inductance (mH)
    public object l_q { get; set; } = 0.35e-1;   // Quadrature axis inductance (mH)
    public object l_m { get; set; } = 1.589e-1;  // Mutual inductance (mH)
    public object l_e { get; set; } = 1.74e-1;   // Excitation inductance (mH)
    public int p { get; set; } = 3;               // Pole pair number
    public object j_rotor { get; set; } = 0.03883; // Moment of inertia of the rotor (kg·m²)

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
    public object r_a { get; set; } = 2;  // Armature circuit resistance (Ohm)
    public object l_a { get; set; } = 1.0e-3; // Armature circuit inductance (Henry)
    public object j_rotor { get; set; } = 0.02; // Moment of inertia of the rotor (kg·m²)

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

public class PermMagnetSynch : MotorBase
{
    public object r_s { get; set; } = 15.55e-1;  // Stator resistance (mOhm)
    public object l_d { get; set; } = 1.66e-1;   // Direct axis inductance (mH)
    public object l_q { get; set; } = 0.35e-1;   // Quadrature axis inductance (mH)
    public int p { get; set; } = 3;               // Pole pair number
    public object j_rotor { get; set; } = 0.03883; // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "PermMagnetSynch";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_s", r_s },
            { "l_d", l_d },
            { "l_q", l_q },
            { "p", p },
            { "j_rotor", j_rotor }
        };
    }
}

public class SeriesDc : MotorBase
{
    public object r_a { get; set; } = 16e-3;      // Armature circuit resistance (Ohm)
    public object r_e { get; set; } = 48e-3;      // Exciting circuit resistance (Ohm)
    public object l_a { get; set; } = 19e-6;      // Armature circuit inductance (Henry)
    public object l_e { get; set; } = 5.4e-3;     // Exciting circuit inductance (Henry)
    public object l_e_prime { get; set; } = 1.7e-3; // Effective excitation inductance (Henry)
    public object j_rotor { get; set; } = 0.025;  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "SeriesDc";

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

public class ShuntDc : MotorBase
{
    public object r_a { get; set; } = 8e-1;       // Armature circuit resistance (Ohm)
    public object r_e { get; set; } = 4e-1;       // Exciting circuit resistance (Ohm)
    public object l_a { get; set; } = 19e-6;      // Armature circuit inductance (Henry)
    public object l_e { get; set; } = 5.4e-3;     // Exciting circuit inductance (Henry)
    public object l_e_prime { get; set; } = 1.7e-3; // Effective excitation inductance (Henry)
    public object j_rotor { get; set; } = 0.025;  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "ShuntDc";

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

public class SynchReluctance : MotorBase
{
    public object r_s { get; set; } = 0.67;        // Stator resistance (Ohm)
    public object l_d { get; set; } = 10.1e-1;     // Direct axis inductance (Henry)
    public object l_q { get; set; } = 4.1e-3;      // Quadrature axis inductance (Henry)
    public int p { get; set; } = 4;                // Pole pair number
    public object j_rotor { get; set; } = 0.8e-4;  // Moment of inertia of the rotor (kg·m²)

    public override string MotorName => "SynchReluctance";

    public override Dictionary<string, object> GetAttributes()
    {
        return new Dictionary<string, object>
        {
            { "r_s", r_s },
            { "l_d", l_d },
            { "l_q", l_q },
            { "p", p },
            { "j_rotor", j_rotor }
        };
    }
}



