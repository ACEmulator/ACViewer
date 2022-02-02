using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        private void UpdateCoinValue(bool sendUpdateMessageIfChanged = true)
        {
            int coins = 0;

            foreach (var coinStack in GetInventoryItemsOfTypeWeenieType(WeenieType.Coin))
                coins += coinStack.Value ?? 0;

            if (sendUpdateMessageIfChanged && CoinValue == coins)
                sendUpdateMessageIfChanged = false;

            CoinValue = coins;

            //if (sendUpdateMessageIfChanged)
                //Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CoinValue, CoinValue ?? 0));
        }
    }
}
