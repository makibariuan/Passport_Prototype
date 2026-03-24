<template>
  <div class="dfa-wrapper">
    <main class="main-split-layout">
      <section class="info-column">
        <h3 class="info-header">OFFICE OF CONSULAR AFFAIRS</h3>
        <h2 class="info-title">Passport Online Registration & Application System</h2>
        <p class="info-disclaimer">
          It is advisable NOT to purchase outbound travel tickets until your passports are actually in your possession.
          The Department of Foreign Affairs will not be responsible for any rebooking charges, loss of income,
          and other financial compensation arising from the applicant's travel arrangements made while the passport has not been released.
        </p>
        <div class="contact-info">
          <span><i class="fas fa-phone"></i> +632 8234 3488</span>
          <span><i class="fas fa-envelope"></i> info@passport.gov.ph</span>
        </div>
      </section>

      <section class="form-column">
        <h2 class="form-title">Register</h2>

        <div class="input-group-row">
          <p class="label-text">Do you have an ePassport issued from 2009 to present?</p>
          <div class="toggle-btns">
            <button :class="{ active: hasEPassport }" @click="hasEPassport = true">Yes</button>
            <button :class="{ active: !hasEPassport }" @click="hasEPassport = false">No</button>
          </div>
        </div>

        <div class="form-field">
          <label>First Name</label>
          <input v-model="firstName" type="text" placeholder="First Name" />
        </div>

        <div class="form-field">
          <label>Middle Name</label>
          <div class="toggle-btns-small">
            <button :class="{ active: hasMiddleName }" @click="hasMiddleName = true">Yes</button>
            <button :class="{ active: !hasMiddleName }" @click="hasMiddleName = false">No</button>
          </div>
          <input v-if="hasMiddleName" v-model="middleName" type="text" placeholder="Middle Name" />
        </div>

        <div class="form-field">
          <label>Last Name</label>
          <input v-model="lastName" type="text" placeholder="Last Name" />
        </div>

        <div class="form-field">
          <label>Suffix</label>
          <input v-model="suffix" type="text" placeholder="Suffix" />
        </div>

        <div class="form-field">
          <label>Email (Will be your Username)</label>
          <div class="input-with-action">
            <input v-model="email" type="email" placeholder="Email Address" />
            <button class="action-btn">Send verification code</button>
          </div>
        </div>

        <div class="form-field">
          <label>Verification Code</label>
          <div class="otp-boxes-static">
            <input v-for="i in 6" :key="i" type="text" maxlength="1" />
          </div>
        </div>

        <div class="form-field">
          <label>Password</label>
          <input v-model="password" type="password" placeholder="Enter Password" />
        </div>

        <div class="form-field">
          <label>Repeat Password</label>
          <input v-model="confirmPassword" type="password" placeholder="Repeat Password" />
        </div>

        <div class="captcha-placeholder">
          <div class="mock-captcha">
            <input type="checkbox" id="robot" />
            <label for="robot">I'm not a robot</label>
          </div>
        </div>

        <div class="terms-check">
          <input type="checkbox" v-model="isTermsAccepted" />
          <span>Read and Accept <a href="#">Terms of Service</a></span>
        </div>

        <button @click="handleRegister" class="submit-register-btn">Sign Up</button>
      </section>
    </main>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  const hasEPassport = ref(false);
  const hasMiddleName = ref(true);
  const firstName = ref('');
  const middleName = ref('');
  const lastName = ref('');
  const suffix = ref('');
  const email = ref('');
  const password = ref('');
  const confirmPassword = ref('');
  const isTermsAccepted = ref(false);

  const handleRegister = () => {
    console.log("Registering user...");
  };
</script>
