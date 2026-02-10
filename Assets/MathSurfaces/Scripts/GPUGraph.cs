using System;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
  [SerializeField]
  ComputeShader computeShader;
  
  [SerializeField]
  Material material;

  [SerializeField]
  Mesh mesh;
  
  private const int maxResolution = 1000;
  [SerializeField, Range(10, maxResolution)] 
  private int resolution;
  
  [SerializeField] 
  FunctionLibrary.FunctionName function;

  public enum TransitionMode { Cycle, Random }

  [SerializeField]
  TransitionMode transitionMode;
  
  [SerializeField, Min((0f))] 
  private float functionDuration = 1f, transitionDuration = 1f;
  
  static readonly int positionsId = Shader.PropertyToID("_Positions");
  static readonly int resolutionId = Shader.PropertyToID("_Resolution");
  static readonly int stepId = Shader.PropertyToID("_Step");
  static readonly int timeId = Shader.PropertyToID("_Time");
  static readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");
  
  private float duration;
  private bool isTransitioning;
  FunctionLibrary.FunctionName transitionFunction;
  private ComputeBuffer positionsBuffer;
  
  private void OnEnable()
  {
    positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
  }

  private void OnDisable()
  {
    positionsBuffer.Release();
    positionsBuffer = null;
  }
  
  private void Start()
  {
    
  }

  void UpdateFunctionOnGPU () {
    float step = 2f / resolution;
    computeShader.SetInt(resolutionId, resolution);
    computeShader.SetFloat(stepId, step);
    computeShader.SetFloat(timeId, Time.time);
    if (isTransitioning) {
      computeShader.SetFloat(
        transitionProgressId,
        Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
      );
    }
    
    
    var kernelIndex = (int)function + (int)(isTransitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
    computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
    
    int groups = Mathf.CeilToInt(resolution / 8f);
    computeShader.Dispatch(kernelIndex, groups, groups, 1);
    
    material.SetBuffer(positionsId, positionsBuffer);
    material.SetFloat(stepId, step);
    var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
    Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
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
    
    UpdateFunctionOnGPU();
  }
  
  void PickNextFunction () {
    function = transitionMode == TransitionMode.Cycle ?
      FunctionLibrary.GetNextFunctionName(function) :
      FunctionLibrary.GetRandomFunctionName(function);
  }
}
