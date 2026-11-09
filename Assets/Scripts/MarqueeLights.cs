// MARQUEELIGHTS.CS  -  put it on the logo (works on an Image OR a Raw Image).
//
// IN ONE SENTENCE: makes the marquee sign feel lit up by pulsing/flickering the whole sign,
// and if you shoot (click) it, it plays a sound and flickers like the bulbs got rattled.
//
// SETUP: make sure "Raycast Target" is ticked on the logo's Image/Raw Image, or clicks won't reach it.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MarqueeLights : MonoBehaviour, IPointerClickHandler   // lets this object detect UI clicks
{
    [Header("Chase (optional, regular Image only)")]
    public Sprite[] frames;
    public float frameTime = 0.15f;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 3f;
    public float minBrightness = 0.6f;

    [Header("Flicker")]
    public bool flicker = true;
    public float flickerChance = 0.01f;

    [Header("When shot")]
    public AudioClip clickSound;        // the gun sound
    public int clickFlickers = 4;       // how many times it blinks
    public float flickerStep = 0.06f;   // how fast each blink is (seconds)

    Graphic graphic;
    Image image;
    float timer;
    int index;

    int blinksLeft;      // counts down the on/off steps of the click flicker
    float blinkTimer;
    bool blinkDark;

    void Start()
    {
        graphic = GetComponent<Graphic>();
        image = GetComponent<Image>();

        if (graphic == null)
            Debug.LogWarning("MarqueeLights: no Image or Raw Image on " + name);
    }

    // Unity calls this automatically when the logo gets clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clickSound);
        blinksLeft = clickFlickers * 2;   // each blink = one dark step + one bright step
        blinkTimer = 0f;
    }

    void Update()
    {
        if (graphic == null) return;

        // CHASE (only works on a regular Image)
        if (image != null && frames != null && frames.Length > 0)
        {
            timer += Time.deltaTime;
            if (timer >= frameTime)
            {
                timer = 0f;
                index = (index + 1) % frames.Length;
                image.sprite = frames[index];
            }
        }

        // PULSE
        float b = 1f;
        if (pulse)
            b = Mathf.Lerp(minBrightness, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        // RANDOM FLICKER
        if (flicker && Random.value < flickerChance)
            b *= 0.7f;

        // CLICK FLICKER: switch dark/bright a few times quickly
        if (blinksLeft > 0)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                blinkTimer = flickerStep;
                blinkDark = !blinkDark;
                blinksLeft--;
            }
            if (blinkDark) b *= 0.35f;
        }
        else
        {
            blinkDark = false;
        }

        graphic.color = new Color(b, b, b, 1f);
    }
}