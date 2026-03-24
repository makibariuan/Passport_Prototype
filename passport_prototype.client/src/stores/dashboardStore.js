import { defineStore } from "pinia";
import api from "@/api"; // Assuming '@/api' is your configured Axios instance

export const useDashboardStore = defineStore("dashboard", {
  // === 1. State: The Core Data and Status ===
  state: () => ({
    // Holds the fetched statistics, initially empty
    statisticsData: {},
    // Tracks loading activity (similar to having local 'isLoading' ref)
    isLoading: false,
    // Holds error messages for display
    error: null,
  }),

  // === 2. Getters: Derived/Computed State ===
  getters: {
    /** Checks if data has been successfully loaded into the store. */
    isLoaded: (state) => Object.keys(state.statisticsData).length > 0,

    // Direct, easy access to specific metric values.
    kitUserCount: (state) => state.statisticsData.kitUserCount || 0,
    systemUserCount: (state) => state.statisticsData.systemUserCount || 0,
    citizenCount: (state) => state.statisticsData.citizenCount || 0,
  },

  // === 3. Actions: Logic and API Calls (Similar to `login` or `checkSession` in auth.js) ===
  actions: {
    /**
     * Fetches dashboard statistics from the API.
     * This handles network logic, loading state, and error management.
     * @param {boolean} force - If true, ignores existing data and forces a fetch.
     */
    async fetchStatistics(force = false) {
      // Guard clause: Prevents unnecessary API calls if data is present and not forced.
      if (!force && this.isLoaded && !this.isLoading) {
        console.log("Statistics data is already loaded. Skipping API call.");
        return;
      }

      this.isLoading = true;
      this.error = null; // Clear previous errors

      try {
        // API Call: This endpoint should return the three counts.
        const res = await api.get('/admin/statistics');
        this.statisticsData = res.data;
      } catch (err) {
        console.error("Dashboard statistics fetch failed:", err);

        this.statisticsData = {}; // Clear previous data on failure
        this.error = "Failed to load dashboard statistics. Check network or authorization.";

        // Throw the error so the calling component can decide on further actions (e.g., logout).
        throw err;
      } finally {
        this.isLoading = false;
      }
    },

    /** Clears the statistics and error state. */
    clearStatistics() {
      this.statisticsData = {};
      this.error = null;
    }
  },
});
