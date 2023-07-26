using UnityEngine;
using UnityEngine.UIElements;

public class SurvivorHUD : MonoBehaviour
{
    [SerializeField] private HealthController survivorHealthController;
    
    private VisualElement _root;
    private VisualElement _healthbarFill;
    
    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _healthbarFill = _root.Q("HealthbarFill");

        survivorHealthController.OnHealthChange += OnSurvivorHealthChange;
    }

    private void OnDisable()
    {
        survivorHealthController.OnHealthChange -= OnSurvivorHealthChange;
    }

    private void OnSurvivorHealthChange(int health, float healthPercentage)
    {
        _healthbarFill.style.width = new StyleLength(new Length(healthPercentage * 100, LengthUnit.Percent));
    }
}
