﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class IQuadtreeTest : PointTest
{
    public TextAsset m_implementation;

    private IQuadtree<Component> m_tree;

    public void Awake()
    {
        System.Type t = System.Type.GetType (m_implementation.name + "`1");

        // TODO: Check type implements IQuadtree<Component>

        System.Type tg = t.MakeGenericType (typeof(Component));

        if (tg != null)
            SetTypeToTest (tg);
    }

    public override string GetName()
    {
        return m_tree.GetType ().Name + "(" + m_sideLength + ")";
    }

    protected void SetTypeToTest(System.Type p_type)
    {
        Dictionary<string, object> context = BuildConstructorContext (m_sideLength);

        m_tree = CreateInstance (p_type, context);
    }

    protected override void ClearTree ()
    {
        m_tree.Clear ();
    }

    protected override void FeedPoint (float p_x, float p_y, Component p_value)
    {
        m_tree.Add(p_x, p_y, p_value);
    }

    protected override Component SearchPoint (float p_x, float p_y)
    {
        return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
    }

    protected Dictionary<string, object> BuildConstructorContext (float p_sideLength)
    {
        Dictionary<string, object> ret = new Dictionary<string, object> ();

        ret.Add ("p_bottomLeftX", 0.0f);
        ret.Add ("p_bottomLeftY", 0.0f);
        ret.Add ("p_topRightX", p_sideLength);
        ret.Add ("p_topRightY", p_sideLength);
        ret.Add ("p_sideLength", p_sideLength);

        return ret;
    }

    protected IQuadtree<Component> CreateInstance(System.Type p_classType, Dictionary<string, object> p_context)
    {
        ConstructorInfo constructor = FindConstructorMatchingContext (p_classType, p_context);

        if (constructor == null)
        {
            Debug.LogError("Could not find a constructor for type '" + p_classType.Name + "' with parameter names contained in (" + DictionaryKeysToString(p_context) + ")");
        }

        return InvokeConstructor (constructor, p_context);
    }

    protected IQuadtree<Component> InvokeConstructor(ConstructorInfo p_constructor,  Dictionary<string, object> p_context)
    {
        return (IQuadtree<Component>) p_constructor.Invoke (GetParametersValues (p_constructor.GetParameters (), p_context));
    }

    protected object[] GetParametersValues( ParameterInfo[] p_parameters, Dictionary<string, object> p_context)
    {
        object[] ret = new object[p_parameters.Length];

        for(int i = 0; i < p_parameters.Length; i++)
        {
            ParameterInfo pi = p_parameters [i];

            ret [i] = p_context [pi.Name];
        }

        return ret;
    }

    protected ConstructorInfo FindConstructorMatchingContext(System.Type p_classType, Dictionary<string, object> p_context)
    {
        ConstructorInfo[] constructors = p_classType.GetConstructors ();

        ConstructorInfo ret = null;
        int maxParameters = 0;

        foreach (ConstructorInfo ci in constructors)
        {
            ParameterInfo[] parameters = ci.GetParameters ();

            int numberOfParameters = parameters.Length;

            if ((numberOfParameters >= maxParameters) &&
                (AllParametersMatch (parameters, p_context) == true))
            {
                maxParameters = numberOfParameters;
                ret = ci;
            }
        }

        return ret;
    }

    protected bool AllParametersMatch(ParameterInfo[] p_parameters, Dictionary<string, object> p_context)
    {
        foreach (ParameterInfo pi in p_parameters)
        {
            if (p_context.ContainsKey (pi.Name) == false)
            {
                Debug.Log ("Parameter " + pi.Name + " not found in context");

                return false;
            }
        }

        return true;
    }

    protected string DictionaryKeysToString(Dictionary<string, object> p_context)
    {
        return string.Join (", ", new List<string> (p_context.Keys).ToArray ());
    }
}