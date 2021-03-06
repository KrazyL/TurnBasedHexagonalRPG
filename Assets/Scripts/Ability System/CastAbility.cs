﻿using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for all cast classes.
/// Casts and tracks status of castables.
/// </summary>
[RequireComponent(typeof (Ability))]
internal abstract class CastAbility : MonoBehaviour
{
    #region Fields

    protected Ability _ability;
    protected bool _isBusy;

    #endregion

    #region Properties

    public bool IsBusy
    {
        get { return _isBusy; }
    }

    #endregion

    #region Other Members

    private void Awake()
    {
        _ability = GetComponent<Ability>();
#if UNITY_EDITOR
        if (GetComponents<CastAbility>().Length > 1) throw new Exception("An ability can only have 1 Cast Mode");
#endif
        Init();
    }

    protected virtual void Init()
    {
    }

    public virtual void Cast()
    {
        if (!_isBusy && _ability && _ability.Target)
        {
            //Ability might not have Owner and if Mana cost is0, we can ignore it.
            if (_ability.ManaCost > 0) _ability.Owner.ManaLeft -= _ability.ManaCost;
            StartCoroutine(CastRoutine());
        }
    }

    private IEnumerator CastRoutine()
    {
        _isBusy = true;
        //Use interfaces instead of send message for type safety.
        Component[] castables = GetComponents(typeof (ICastable));
        for (int i = 0; i < castables.Length; i++)
        {
            ((ICastable)castables[i]).Cast();
        }

        while (true)
        {
            bool isAnyBusy = false;
            for (int i = 0; i < castables.Length; i++)
            {
                if (((ICastable)castables[i]).IsBusy())
                {
                    isAnyBusy = true;
                    break;
                }
            }
            if (!isAnyBusy) break;
            yield return null;
        }

        _isBusy = false;

        _ability.AbilityCastCompleted(true);
    }

    #endregion
}