public interface IDestructible
{
    string GetObjectTag();
    int GetHealth();
    int GetMaxHealth();
    void ApplyDamage(int damage, bool instadeath = false);
}
