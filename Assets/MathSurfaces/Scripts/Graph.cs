using System;
using UnityEngine;

public class Graph : MonoBehaviour
{
  [SerializeField] 
  private Transform pointPrefab;
  
  [SerializeField, Range(10,100)] 
  private int resolution;
  
  [SerializeField] 
  FunctionLibrary.FunctionName function;

  public enum TransitionMode { Cycle, Random }

  [SerializeField]
  TransitionMode transitionMode;
  
  [SerializeField, Min((0f))] 
  private float functionDuration = 1f, transitionDuration = 1f;
  
  private Transform[] points;
  private float duration;
  private bool isTransitioning;
  FunctionLibrary.FunctionName transitionFunction;
  
  private void Awake()
  {
    float step = 2f / resolution;
    var scale = Vector3.one * step;
    points = new Transform[resolution * resolution];
    for (int i = 0; i < points.Length; i++)
    {
      Transform point = points[i] = Instantiate(pointPrefab);
      point.localScale = scale;
      point.SetParent(transform, false);
    }
  }

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Start()
  {
    
  }

  // Update is called once per frame
  private void Update()
  {
    duration += Time.deltaTime;
    if (isTransitioning) {
      if (duration >= transitionDuration) {
        duration -= transitionDuration;
        isTransitioning = false;
      }
    }
    else if (duration >= functionDuration)
    {
      duration -= functionDuration;
      isTransitioning = true;
      transitionFunction = function;
      PickNextFunction();
    }

    if (isTransitioning) {
      UpdateFunctionTransition();
    }
    else {
      UpdateFunction();
    }
  }

  private void UpdateFunction()
  {
    FunctionLibrary.Function f = FunctionLibrary.GetFunction(function); 
    float time = Time.time;
    float step = 2f / resolution;
    float v = 0.5f * step - 1f;
    for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
      if (x == resolution) {
        x = 0;
        z += 1;
        v = (z + 0.5f) * step - 1f;
      }
      float u = (x + 0.5f) * step - 1f;
      points[i].localPosition = f(u, v, time);
    }
  }

  private void UpdateFunctionTransition()
  {
    FunctionLibrary.Function
      from = FunctionLibrary.GetFunction(transitionFunction),
      to = FunctionLibrary.GetFunction(function);
    float progress = duration / transitionDuration;
    float time = Time.time;
    float step = 2f / resolution;
    float v = 0.5f * step - 1f;
    for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
      if (x == resolution) {
        x = 0;
        z += 1;
        v = (z + 0.5f) * step - 1f;
      }
      float u = (x + 0.5f) * step - 1f;
      points[i].localPosition = FunctionLibrary.Morph(
        u, v, time, from, to, progress
      );
    }
  }

  void PickNextFunction () {
    function = transitionMode == TransitionMode.Cycle ?
      FunctionLibrary.GetNextFunctionName(function) :
      FunctionLibrary.GetRandomFunctionName(function);
  }
  
}