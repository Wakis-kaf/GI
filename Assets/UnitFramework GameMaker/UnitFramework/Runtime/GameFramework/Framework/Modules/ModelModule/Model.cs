using System;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class Model
    {
        private BlackBoard m_Owner ;
        private int mIntValue;
        private long mLongValue;
        private float mFloatValue;
        private double mDoubleValue;
        private bool mBoolValue;
        private object mObjectValue;
        private Color mColorValue;
        private Rect mRectValue;
        private Vector2 mVector2Value;
        private Vector3 mVector3Value;
        private Quaternion mQuaternionValue;
        private Transform mTransformValue;
        private GameObject mGameObjectValue;
        private AnimationCurve mAnimationCurveValue;
        public event Action<int> OnIntValueChanged;
        public event Action<long> OnLongValueChanged;
        public event Action<float> OnFloatValueChanged;
        public event Action<Double> OnDoubleValueChanged;
        public event Action<bool> OnBooleanValueChanged;
        public event Action<object> OnObjectValueChanged;
        public event Action<Color> OnColorValueChanged;
        public event Action<Rect> OnRectValueChanged;
        public event Action<Vector2> OnVector2ValueChanged;
        public event Action<Vector3> OnVector3ValueChanged;
        public event Action<Quaternion> OnQuaternionValueChanged;
        public event Action<Transform> OnTransformChanged; 
        public event Action<GameObject> OnGameObjectChanged;
        public event Action<AnimationCurve> OnAnimationCurveChanged;
        
        public int intValue
        {
            get => mIntValue;
            set
            {
                if (mIntValue != value)
                {
                    mIntValue = value;
                    OnIntValueChanged?.Invoke(value);
                }
                
            }
        }
        public long longValue
        {
            get => mLongValue;
            set
            {
                if (mLongValue != value)
                {
                    mLongValue = value;
                    OnLongValueChanged?.Invoke(value);
                }
                
            }
        }
        public float floatValue{
            get => mFloatValue;
            set
            {
                if (mFloatValue != value)
                {
                    mFloatValue = value;
                    OnFloatValueChanged?.Invoke(value);
                }
                
            }
        }
        public double doubleValue{
            get => mDoubleValue;
            set
            {
                if (mDoubleValue != value)
                {
                    mDoubleValue = value;
                    OnDoubleValueChanged?.Invoke(value);
                }
                
            }
        }
        public bool boolValue{
            get => mBoolValue;
            set
            {
                if (mBoolValue != value)
                {
                    mBoolValue = value;
                    OnBooleanValueChanged?.Invoke(value);
                }
                
            }
        }
        public object objectValue{
            get => mObjectValue;
            set
            {
                if (mObjectValue != value)
                {
                    mObjectValue = value;
                    OnObjectValueChanged?.Invoke(value);
                }
                
            }
        }
        public Color colorValue{
            get => mColorValue;
            set
            {
                if (mColorValue != value)
                {
                    mColorValue = value;
                    OnColorValueChanged?.Invoke(value);
                }
                
            }
        }
        public Rect rectValue{
            get => mRectValue;
            set
            {
                if (mRectValue != value)
                {
                    mRectValue = value;
                    OnRectValueChanged?.Invoke(value);
                }
                
            }
        }
        public Vector2 vector2Value{
            get => mVector2Value;
            set
            {
                if (mVector2Value != value)
                {
                    mVector2Value = value;
                    OnVector2ValueChanged?.Invoke(value);
                }
                
            }
        }
        public Vector3 vector3Value{
            get => mVector3Value;
            set
            {
                if (mVector3Value != value)
                {
                    mVector3Value = value;
                    OnVector3ValueChanged?.Invoke(value);
                }
                
            }
        }
        public Quaternion quaternionValue{
            get => mQuaternionValue;
            set
            {
                if (mQuaternionValue != value)
                {
                    mQuaternionValue = value;
                    OnQuaternionValueChanged?.Invoke(value);
                }
                
            }
        }
        public Transform transformValue{
            get => mTransformValue;
            set
            {
                if (mTransformValue != value)
                {
                    mTransformValue = value;
                    OnTransformChanged?.Invoke(value);
                }
                
            }
        }
        public GameObject gameObjectValue{
            get => mGameObjectValue;
            set
            {
                if (mGameObjectValue != value)
                {
                    mGameObjectValue = value;
                    OnGameObjectChanged?.Invoke(value);
                }
                
            }
        }
        public AnimationCurve animationCurveValue{
            get => mAnimationCurveValue;
            set
            {
                if (!mAnimationCurveValue.Equals(value))
                {
                    mAnimationCurveValue = value;
                    OnAnimationCurveChanged?.Invoke(value);
                }
                
            }
        }
        public BlackBoard Owner => m_Owner;
        public Model(BlackBoard owner)
        {
            m_Owner = owner;
        }

    }
}