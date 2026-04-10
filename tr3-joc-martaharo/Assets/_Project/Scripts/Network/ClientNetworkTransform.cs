using Unity.Netcode.Components;
using UnityEngine;

// =================================================================================
// SCRIPT: ClientNetworkTransform
// UBICACIÓ: Assets/_Project/Scripts/Network/
// DESCRIPCIÓ: Estén NetworkTransform per permetre autoritat del client (propietari).
//             Això evita el rubber-banding en el moviment.
// =================================================================================

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    /// <summary>
    /// Determina si el servidor té l'autoritat. 
    /// Retornant false, el Client propietari pot moure's directament.
    /// </summary>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
