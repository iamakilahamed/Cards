using UnityEngine;
using TMPro;
using GameShared.Cards;

namespace GameClient.Cards
{
    /// <summary>
    /// The purely visual representation of a card. 
    /// Controlled entirely by the ClientMatchManager and VisualQueue.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _powerText;
        [SerializeField] private TextMeshProUGUI _nameText;

        public int NetworkId { get; private set; }

        /// <summary>
        /// Populates the UI elements with raw data from the server.
        /// </summary>
        public void Initialize(CardInstance instanceData, CardDefinition blueprint)
        {
            NetworkId = instanceData.NetworkId;

            // We would also load a Sprite based on the blueprint ID here.
            _nameText.text = blueprint.CardName;

            // We use the instance data for Cost and Power in case they were modified by abilities.
            UpdateStats(instanceData.CurrentCost, instanceData.CurrentPower);
        }

        public void UpdateStats(int currentCost, int currentPower)
        {
            _costText.text = currentCost.ToString();
            _powerText.text = currentPower.ToString();
        }
    }
}