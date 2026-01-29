public interface IAbilityReceiver
{
    bool HasAbility(string id);
    void GrantAbility(string id);
}