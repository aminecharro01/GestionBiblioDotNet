# 📚 GestionBiblio - Système de Gestion de Bibliothèque (v1.0.0)

Une application web complète pour la gestion d'une bibliothèque, développée avec ASP.NET Core MVC.

## 🌟 Fonctionnalités

### 👨‍💼 Pour les Administrateurs
*   **Tableau de Bord** : Vue d'ensemble avec statistiques (Livres, Membres, Emprunts actifs/retard, Réservations) et graphiques.
*   **Gestion des Livres** : Ajouter, modifier, supprimer des livres avec gestion des catégories et upload d'images.
*   **Gestion des Membres** : Gérer les inscriptions et les profils des membres.
*   **Suivi des Emprunts** : Enregistrer les prêts, gérer les retours, visualiser les retards.
*   **Gestion des Amendes** : Créer et suivre le paiement des amendes pour les retards.
*   **Export des Données** : Exportation des listes (ex: amendes) au format CSV.

### 👤 Pour les Membres
*   **Catalogue** : Rechercher et consulter les livres disponibles.
*   **Espace Personnel** : Voir ses emprunts en cours, son historique et ses réservations.
*   **Réservation** : Réserver un livre si celui-ci n'est pas disponible.

## 🛠️ Stack Technique

*   **Framework** : ASP.NET Core 8.0 (MVC)
*   **Langage** : C# 10+
*   **Base de Données** : SQL Server (via Entity Framework Core 8)
*   **ORM** : Entity Framework Core (Code-First)
*   **Authentification** : ASP.NET Core Identity
*   **Frontend** :
    *   Razor Views (.cshtml)
    *   **Tailwind CSS** (via CDN) pour le styling moderne.
    *   **Glassmorphism UI** (Design personnalisé avec effets de transparence).
    *   Bootstrap Icons.
    *   Chart.js pour la visualisation des données.

## 🗄️ Conception de la Base de Données

Le schéma relationnel comprend les entités principales suivantes :

*   **Livre** : Titre, Auteur, ISBN, Image, Stock, Catégorie.
*   **Membre** : Informations personnelles (liées à Identity User).
*   **Emprunt** : Lien entre Livre et Membre avec Date d'emprunt, Date de retour prévue et effective.
*   **Reservation** : File d'attente pour les livres hors stock.
*   **Amende** : Pénalités financières liées aux emprunts en retard.
*   **Categorie** : Classification des livres.

## 🚀 Installation et Configuration

### Prérequis
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express ou Developer)
*   Visual Studio 2022 ou VS Code

### Étapes d'installation

1.  **Cloner le dépôt**
    ```bash
    git clone https://github.com/aminecharro01/GestionBiblioDotNet.git
    cd GestionBiblioDotNet
    ```

2.  **Configurer la Base de Données**
    Ouvrez `appsettings.json` et modifiez la chaîne de connexion `DefaultConnection` si nécessaire pour pointer vers votre instance SQL Server locale.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LibraryDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    }
    ```

3.  **Appliquer les Migrations**
    Crée la base de données et les tables nécessaires.
    ```bash
    dotnet ef database update
    ```

4.  **Lancer l'Application**
    ```bash
    dotnet run
    ```
    L'application sera accessible à l'adresse `https://localhost:7152` (ou le port indiqué dans la console).

5.  **Compte Administrateur par Défaut**
    Au premier lancement, un compte administrateur est créé (voir `Program.cs` pour les détails de seeding) :
    *   **Email** : `admin@gestionbiblio.com`
    *   **Mot de passe** : `Admin123!`

## 🎨 Design

L'application utilise une interface **Glassmorphism** moderne :
*   Arrière-plan immersif fixe.
*   Cartes et conteneurs semi-transparents avec flou (`backdrop-blur`).
*   Typographie soignée (Police 'Inter').
*   Palette de couleurs cohérente (Indigo/Slate).

## 👤 Auteur

**Amine Charro**
[Profil GitHub](https://github.com/aminecharro01)
