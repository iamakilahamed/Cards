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
        [Header("Data")]
        public int NetworkId { get; private set; }

        [Header("Visuals")]
        [SerializeField] private Transform _visualPivot;
        [SerializeField] private TextMeshPro _costText;
        [SerializeField] private TextMeshPro _powerText;
        [SerializeField] private TextMeshPro _nameText;

        public Transform VisualPivot => _visualPivot;

        public void Initialize(CardInstance cardData, CardDefinition blueprint)
        {
            NetworkId = cardData.NetworkId;

            if (_costText != null) _costText.text = cardData.CurrentCost.ToString();
            if (_powerText != null) _powerText.text = cardData.CurrentPower.ToString();
            if (_nameText != null) _nameText.text = blueprint.CardName;

            SetFaceDown(true);
        }

        public void SetFaceDown(bool isFaceDown)
        {
            float targetYRotation = isFaceDown ? 180f : 0f;
            _visualPivot.localEulerAngles = new Vector3(0, targetYRotation, 0);
        }

        public void UpdatePower(int newPower)
        {
            if (_powerText != null) _powerText.text = newPower.ToString();
        }
    }
}