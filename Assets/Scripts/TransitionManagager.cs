using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManagager : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject magazineCanvas;
    public GameObject photoCanvas;
    
    [Header("Transition Settings")]
    public float transitionDuration = 0.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private RectTransform magazineRectTransform;
    private RectTransform photoRectTransform;
    private bool isTransitioning = false;
    
    private void Start()
    {
        // Get the RectTransforms
        magazineRectTransform = magazineCanvas.GetComponent<RectTransform>();
        photoRectTransform = photoCanvas.GetComponent<RectTransform>();
        
        // Set up initial state
        SetInitialState();
    }
    
    private void SetInitialState()
    {
        // Start with photo mode active and magazine inactive
        photoCanvas.SetActive(true);
        magazineCanvas.SetActive(false);
    }
    
    // Called by the Magazine Mode button
    public void SwitchToMagazineMode()
    {
        if (isTransitioning || magazineCanvas.activeSelf)
            return;
            
        isTransitioning = true;
        
        // Activate magazine canvas but position it offscreen to the right
        magazineCanvas.SetActive(true);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        magazineRectTransform.anchoredPosition = new Vector2(screenSize.x, 0);
        
        // Start the transition animation
        StartCoroutine(SlideInFromRight(magazineRectTransform));
        StartCoroutine(SlideOutToLeft(photoRectTransform));
    }
    
    // Called by the Photo Mode button
    public void SwitchToPhotoMode()
    {
        if (isTransitioning || photoCanvas.activeSelf)
            return;
            
        isTransitioning = true;
        
        // Activate photo canvas but position it offscreen to the left
        photoCanvas.SetActive(true);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        photoRectTransform.anchoredPosition = new Vector2(-screenSize.x, 0);
        
        // Start the transition animation
        StartCoroutine(SlideInFromLeft(photoRectTransform));
        StartCoroutine(SlideOutToRight(magazineRectTransform));
    }
    
    private IEnumerator SlideInFromRight(RectTransform rectTransform)
    {
        float time = 0;
        Vector2 startPos = new Vector2(Screen.width, 0);
        Vector2 endPos = Vector2.zero;
        
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = transitionCurve.Evaluate(time / transitionDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
    }
    
    private IEnumerator SlideInFromLeft(RectTransform rectTransform)
    {
        float time = 0;
        Vector2 startPos = new Vector2(-Screen.width, 0);
        Vector2 endPos = Vector2.zero;
        
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = transitionCurve.Evaluate(time / transitionDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
    }
    
    private IEnumerator SlideOutToLeft(RectTransform rectTransform)
    {
        float time = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-Screen.width, 0);
        
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = transitionCurve.Evaluate(time / transitionDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        rectTransform.gameObject.SetActive(false);
        rectTransform.anchoredPosition = startPos;
        isTransitioning = false;
    }
    
    private IEnumerator SlideOutToRight(RectTransform rectTransform)
    {
        float time = 0;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(Screen.width, 0);
        
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = transitionCurve.Evaluate(time / transitionDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        rectTransform.gameObject.SetActive(false);
        rectTransform.anchoredPosition = startPos;
        isTransitioning = false;
    }
}
