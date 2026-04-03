namespace ProjectG.DomainLayer.Entities.Enums
{
    /// <summary>
    /// Dynamic AH akışında hangi varyant ailesinin ağırlıklı seçileceği (manuel yüzde yok).
    /// </summary>
    public enum DynamicAhFlowMode
    {
        /// <summary>V1 (cancel giriş) + V2 (PostDelayCancelPost) ağırlıklı; V3–V4 daha düşük.</summary>
        V1V2Heavy = 0,

        /// <summary>V3 (PostThenCancel) + V4 (PostDelayThenCancel) ağırlıklı; V1–V2 daha düşük.</summary>
        V3V4Heavy = 1,
    }
}
