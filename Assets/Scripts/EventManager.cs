using System;

public static class EventManager
{
    // Parte de Vida
    public static event Action<int, int> OnHealthChanged;

    // Parte de Inventario
    public static event Action<string> OnItemCollected;

    // Parte de Salvamento
    public static event Action OnSaveRequested;
    public static event Action OnLoadRequested;

    // Métodos de disparo
    public static void TriggerHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
    public static void TriggerItemCollected(string item) => OnItemCollected?.Invoke(item);
    public static void TriggerSaveRequested() => OnSaveRequested?.Invoke();
    public static void TriggerLoadRequested() => OnLoadRequested?.Invoke();
}