using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace RAXY.Utility.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextTyper : MonoBehaviour
    {
        [SerializeField, Range(0.001f, 0.1f)]
        private float charInterval = 0.03f;

        private TextMeshProUGUI tmp;
        private Coroutine typingRoutine;
        private bool isTyping;

        public bool IsTyping => isTyping;

        private void Awake()
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Mulai efek typewriter dengan teks baru.
        /// </summary>
        [Button]
        public void StartTyping(string text)
        {
            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            tmp.text = text;
            tmp.maxVisibleCharacters = 0;

            typingRoutine = StartCoroutine(TypeRoutine());
        }

        /// <summary>
        /// Tampilkan seluruh teks langsung (untuk skip).
        /// </summary>
        [Button]
        public void ShowAllInstant()
        {
            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            tmp.maxVisibleCharacters = tmp.textInfo.characterCount;
            isTyping = false;
        }

        private IEnumerator TypeRoutine()
        {
            isTyping = true;
            tmp.ForceMeshUpdate();
            int totalChars = tmp.textInfo.characterCount;
            int visibleChars = 0;

            while (visibleChars < totalChars)
            {
                visibleChars++;
                tmp.maxVisibleCharacters = visibleChars;
                yield return new WaitForSeconds(charInterval);
            }

            isTyping = false;
        }
    }
}
