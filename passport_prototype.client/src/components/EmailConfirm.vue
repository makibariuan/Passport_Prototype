<template>
  <div class="confirm-container auth-page-wrapper">
    <div class="confirm-box auth-box-style">
      <h2 class="auth-title">Email Confirmation</h2>

      <div v-if="loading" class="loading-state">
        <LoadingDialog :visible="true" />
        <p>Verifying your account...</p>
      </div>

      <div v-else>
        <div class="dialog-message-container">
          <p v-if="notice === 'pending'" class="dialog-message-line info">
            📧 Please confirm your email. Check your inbox (or spam). Then login again.
          </p>

          <p v-else-if="success" class="dialog-message-line success">
            ✅ {{ message }}
          </p>

          <p v-else class="dialog-message-line error">
            ❌ {{ message }}
          </p>
        </div>

        <div class="button-row">
          <button v-if="expired" @click="resendEmail" class="auth-btn resend-btn">
            Resend Email
          </button>

          <button @click="goToLogin" class="auth-btn">
            Go to Login
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from "vue";
  import { useRoute, useRouter } from "vue-router";
  import api from "@/api";

  const route = useRoute();
  const router = useRouter();

  const loading = ref(true);
  const success = ref(false);
  const message = ref("");
  const expired = ref(false);
  const notice = route.query.notice; // "pending" from login redirect

  onMounted(async () => {
    const token = route.query.token;
    const email = route.query.email;

    // Case: user redirected from login
    if (notice === "pending" && !token && !email) {
      loading.value = false;
      success.value = false;
      message.value = "Please confirm your email before logging in.";
      return;
    }

    // Case: clicked link in email
    if (token && email) {
      try {
        const res = await api.get("/auth/confirm-email", {
          params: { token, email },
        });
        message.value = res.data.message || "Email confirmed successfully!";
        success.value = true;
      } catch (err) {
        const msg = err.response?.data?.message || "Email confirmation failed.";
        message.value = msg;
        success.value = false;

        if (msg.toLowerCase().includes("expired")) {
          expired.value = true;
        }
      } finally {
        loading.value = false;
      }
    } else {
      // Invalid link
      loading.value = false;
      success.value = false;
      message.value = "Invalid confirmation link.";
    }
  });

  const goToLogin = () => {
    router.push({ path: "/login" });
  };

  const resendEmail = async () => {
    try {
      await api.post("/auth/resend-confirmation", { email: route.query.email });
      message.value = "📧 A new confirmation email has been sent!";
      success.value = true;
      expired.value = false;
    } catch (err) {
      message.value =
        err.response?.data?.message || "Failed to resend confirmation email.";
    }
  };
</script>

<style scoped>
  /* Main Background wrapper to match AuthPage */
  .confirm-container {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    background: linear-gradient(135deg, #002a57 0%, #004085 100%);
    padding: 20px;
  }

  /* Matching the Auth Box / Dialog Box style */
  .confirm-box {
    background: white;
    padding: 40px;
    border-radius: 20px;
    text-align: center;
    box-shadow: 0 15px 40px rgba(0, 0, 0, 0.3);
    max-width: 500px;
    width: 100%;
  }

  /* Matching Auth Title */
  .auth-title {
    font-size: 2.2rem;
    font-weight: 800;
    color: #002a57;
    margin-bottom: 30px;
  }

  /* Blue Message Container from DialogBox.vue */
  .dialog-message-container {
    margin: 20px 0 30px 0;
    padding: 20px;
    background-color: #e3f2fd;
    border: 2px solid #90caf9;
    border-radius: 12px;
    text-align: left;
  }

  .dialog-message-line {
    font-size: 1.1rem;
    font-weight: 600;
    line-height: 1.5;
    margin: 0;
  }

  /* Status specific colors */
  .success {
    color: #2e7d32;
  }

  .error {
    color: #c62828;
  }

  .info {
    color: #002a57;
  }

  /* Button Layout */
  .button-row {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  /* Matching Auth Button */
  .auth-btn {
    width: 100%;
    padding: 16px;
    border: none;
    border-radius: 12px;
    font-size: 1.1rem;
    font-weight: 700;
    color: white;
    background: linear-gradient(180deg, #4a698d, #3b506b);
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  }

    .auth-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
      background: #3b506b;
    }

  /* Distinct color for Resend to avoid confusion */
  .resend-btn {
    background: linear-gradient(180deg, #6c757d, #495057);
  }

  .loading-state {
    padding: 20px;
    color: #002a57;
    font-weight: 600;
  }
</style>
