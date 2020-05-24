using UnityEngine;
using UnityEngine.UI;

namespace MPJamPack {
    public class SpriteSheetAnimator : MonoBehaviour
    {
        [SerializeField]
        private AnimType type;
        private enum AnimType { Loop, PingPong, BackwarpLoop }

        [SerializeField]
        private bool sameInterval = true;
        [SerializeField]
        private float interval = 0.2f;

        [SerializeField]
        private KeyPoint[] keyPoints;
        private int keyPointIndex;
        private bool indexForward = true;

        private SpriteRenderer spriteRenderer;
        private Image image;

        private Timer timer;

        private Sprite sprite {
            set {
                if (spriteRenderer != null) spriteRenderer.sprite = value;
                else image.sprite = value;
            }
        }

        private void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            image = GetComponent<Image>();

            if (sameInterval) timer = new Timer(interval);
            else timer = new Timer(keyPoints[0].Interval);

            switch (type) {
                case AnimType.Loop:
                case AnimType.PingPong:
                    sprite = keyPoints[0].Sprite;
                    keyPointIndex = 1;
                    break;
                case AnimType.BackwarpLoop:
                    sprite = keyPoints[keyPoints.Length - 1].Sprite;
                    keyPointIndex = keyPoints.Length - 2;
                    indexForward = false;
                    break;
            }
        }

        private void Update() {
            if (timer.UpdateEnd) {
                timer.Reset();
                if (!sameInterval) timer.TargetTime = keyPoints[keyPointIndex].Interval;

                sprite = keyPoints[keyPointIndex].Sprite;

                if (indexForward) {
                    keyPointIndex++;
                    if (keyPointIndex >= keyPoints.Length) {
                        if (type == AnimType.Loop) keyPointIndex = 0;
                        else {
                            indexForward = false;
                            keyPointIndex = keyPoints.Length - 2;
                        }
                    }
                }
                else {
                    keyPointIndex--;
                    if (keyPointIndex < 0) {
                        if (type == AnimType.BackwarpLoop) keyPointIndex = keyPoints.Length - 1;
                        else {
                            indexForward = true;
                            keyPointIndex = 1;
                        }
                    }
                }
            }
        }


        [System.Serializable]
        private struct KeyPoint {
            public Sprite Sprite;
            public float Interval;
        }
    }
}