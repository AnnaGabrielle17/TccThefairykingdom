using UnityEngine;
using System.Collections.Generic;

public class PlayerSpeedBuff : MonoBehaviour
{
   class Buff { public float mult; public float endTime; }

    private List<Buff> buffs = new List<Buff>();
    private FairyMovement fm;
    private float baseSpeed;
    private bool baseCaptured = false;

    private void Awake()
    {
        fm = GetComponent<FairyMovement>();
        if (fm == null)
            Debug.LogWarning("PlayerSpeedBuff: FairyMovement não encontrado no GameObject.");
    }

    private void Update()
    {
        if (fm == null) return;

        // captura baseSpeed uma vez (valor inicial do componente)
        if (!baseCaptured)
        {
            baseSpeed = fm.speed;
            baseCaptured = true;
        }

        float now = Time.time;
        // remove buffs expirados
        for (int i = buffs.Count - 1; i >= 0; i--)
            if (buffs[i].endTime > 0f && buffs[i].endTime <= now)
                buffs.RemoveAt(i);

        // calcula produto dos multiplicadores
        float prod = 1f;
        for (int i = 0; i < buffs.Count; i++) prod *= buffs[i].mult;

        // aplica sem perder o baseSpeed original
        fm.speed = baseSpeed * prod;
    }

    // adiciona buff (mult > 1 aumenta). duracao em segundos. duracao=0 => permanente.
    public void AddMultiplier(float mult, float duracao)
    {
        if (Mathf.Approximately(mult, 1f)) return;
        Buff b = new Buff();
        b.mult = mult;
        b.endTime = (duracao > 0f) ? Time.time + duracao : 0f;
        buffs.Add(b);
    }

    // remove todos buffs temporários
    public void ClearTemporaryBuffs()
    {
        buffs.RemoveAll(b => b.endTime > 0f);
    }

    // restaura o speed ao base (útil caso queiras reset manual)
    public void ResetToBase()
    {
        if (fm == null) return;
        fm.speed = baseSpeed;
        buffs.Clear();
    }

    // opcional: troca o baseSpeed manualmente (se você quiser definir outro base)
    public void SetBaseSpeed(float newBase)
    {
        baseSpeed = newBase;
        baseCaptured = true;
    }
}

