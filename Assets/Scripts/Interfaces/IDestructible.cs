public interface IDestructible
{
    int GetHealth();
    int GetMaxHealth();
    void ApplyDamage(int damage, bool instadeath = false);
}
