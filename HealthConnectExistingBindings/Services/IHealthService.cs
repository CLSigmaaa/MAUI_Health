using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthConnectExistingBindings.Services
{
    public interface IHealthService
    {
        /// <summary>
        /// Initialise le client Health Connect
        /// </summary>
        /// <returns>True si l'initialisation est réussie</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Vérifie si toutes les permissions nécessaires sont accordées
        /// </summary>
        /// <returns>True si toutes les permissions sont accordées</returns>
        Task<bool> HasAllPermissionsAsync();

        /// <summary>
        /// Demande les permissions nécessaires à l'utilisateur
        /// </summary>
        /// <returns>True si les permissions sont accordées</returns>
        bool RequestPermissionsAsync();

        /// <summary>
        /// Lit les données de pas pour aujourd'hui
        /// </summary>
        /// <returns>Nombre total de pas pour aujourd'hui</returns>
        Task<int> ReadStepsTodayAsync();

        /// <summary>
        /// Ajoute des données de pas pour aujourd'hui
        /// </summary>
        /// <param name="steps">Nombre de pas à ajouter</param>
        /// <returns>True si l'ajout est réussi</returns>
        Task<bool> AddStepsTodayAsync(int steps);

        /// <summary>
        /// Vérifie le statut du SDK de santé
        /// </summary>
        /// <returns>True si le SDK est disponible</returns>
        bool CheckSdkStatus();
    }
}
