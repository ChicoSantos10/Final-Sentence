using Player;

namespace Combat
{
    public interface ICombatUnit
    {
        Stats Stats { get; }
        void TakeDamage(float damage);
    }
}
