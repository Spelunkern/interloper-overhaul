namespace InterloperOverhaul;

// Recipe salt amounts remain native. This tracks the carried mass of salt
// deposited from lighter jars and bags without changing their curing yields.
public sealed class SaltMassRecord
{
    public double QuantityKg { get; set; }
    public double MassKg { get; set; }
    public const double LegacyScale=0.10/0.25;
    public void Reconcile(double quantity)
    {
        quantity=Math.Max(0,quantity);
        if (QuantityKg<=0) MassKg=quantity*LegacyScale;
        else if (quantity<QuantityKg) MassKg*=quantity/QuantityKg;
        else if (quantity>QuantityKg) MassKg+=(quantity-QuantityKg)*LegacyScale;
        QuantityKg=quantity;
        if (quantity==0) MassKg=0;
    }
    public void Deposit(double quantity,double mass)
    {
        QuantityKg+=Math.Max(0,quantity);
        MassKg+=Math.Max(0,mass);
    }
}
