<template>
  <div class="auth-page-wrapper">

    <div class="auth-main-layout">

      <div class="logo-container">

        <img src="@/assets/makati-logo.png" alt="Makati Logo" style="width:140px; height:auto; margin-bottom:16px; border-radius: 50%; border: 4px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.1);" />
        <div class="page-title">
          <h2>Welcome to </h2>
          <h1>ONE CGM Portal</h1>
        </div>
      </div>

      <div class="auth-container">
        <div class="auth-box">


          <h2 class="auth-title">{{ isLogin ? "Sign In" : "Create Account" }}</h2>

          <div v-if="isLogin && loginStep === 'credentials'">
            <div class="auth-input-group">
              <p class="auth-input-label">Email Address</p>
              <input v-model="username" type="email" placeholder="Email Address" class="auth-input" />
            </div>
            <p class="auth-input-label">Password</p>

            <div class="password-wrapper">

              <!--// ori-->
              <!--<input :type="showPassword ? 'text' : 'password'" v-model="password" placeholder="Password" class="auth-input" />
              <span class="toggle-password" @click="showPassword = !showPassword">
                <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </span>-->
              <!--// ori-->
              <!--//this for now-->

              <input :type="showPassword ? 'text' : 'password'" v-model="password" placeholder="Password" class="auth-input" />
              <span class="toggle-password" @click="showPassword = !showPassword">
                <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </span>
            </div>


            <p class="forgot-password link" @click="openForgotPasswordDialog">Forgot password?</p>
            <button @click="handleLogin" class="auth-btn" :disabled="!canLogin || isLoading">Sign In</button>

            <p class="text-center">

              Don't have an account?

              <span class="link" @click="isLogin = false">Create account</span>

            </p>



            <div class="spacer"></div>
          </div>

          <div v-else-if="!isLogin">

            <div class="register-grid-container">

              <div style="grid-column: 1 / -1; display: flex; align-items: flex-end; gap: 15px;">
                <div class="terms-wrapper" style="font-weight: 600; flex-grow: 1;">
                  <input type="checkbox" v-model="isEmployee" />
                  Register as Employee
                </div>

                <div v-if="isEmployee" style="flex-grow: 2;">
                  <p class="auth-input-label">Employee ID</p>
                  <!--<input v-model="employeeID" type="text" placeholder="Employee ID" class="auth-input" />-->
                  <input v-model="employeeID"
                         type="text"
                         placeholder="Employee ID"
                         class="auth-input"
                         :class="{ 'error-border': fieldErrors.employeeID }" />
                </div>
              </div>

              <div>
                <p class="auth-input-label">First Name</p>
                <!--<input v-model="firstName" type="text" placeholder="First Name" class="auth-input" />-->
                <input v-model="firstName"
                       type="text"
                       placeholder="First Name"
                       class="auth-input"
                       :class="{ 'error-border': fieldErrors.firstName }" />
              </div>
              <div>
                <p class="auth-input-label">Last Name</p>
                <!--<input v-model="lastName" type="text" placeholder="Last Name" class="auth-input" />-->
                <input v-model="lastName"
                       type="text"
                       placeholder="Last Name"
                       class="auth-input"
                       :class="{ 'error-border': fieldErrors.lastName }" />
              </div>

              <div>
                <p class="auth-input-label">Birthday</p>
                <!--<input v-model="birthday" type="date" placeholder="mm/dd/yyyy" class="auth-input" />-->
                <input v-model="birthday"
                       type="date"
                       placeholder="mm/dd/yyyy"
                       class="auth-input"
                       :class="{ 'error-border': fieldErrors.birthday }" />
              </div>

              <div>
                <p class="auth-input-label">Email Address (Will be your username)</p>
                <!--<input v-model="email" type="email" placeholder="juandelacruz@makati.com.ph" class="auth-input" />-->
                <input v-model="email"
                       type="email"
                       placeholder="juandelacruz@makati.com.ph"
                       class="auth-input"
                       :class="{ 'error-border': fieldErrors.email }" />
              </div>

              <div>
                <p class="auth-input-label">Password</p>
                <div class="password-wrapper">
                  <input :type="showPassword ? 'text' : 'password'" v-model="password" placeholder="Password" class="auth-input" @input="validatePassword" />
                  <span class="toggle-password" @click="showPassword = !showPassword">
                    <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                  </span>
                </div>
              </div>


              <div>
                <p class="auth-input-label">Confirm Password</p>
                <div class="password-wrapper">
                  <input :type="showConfirm ? 'text' : 'password'" v-model="confirmPassword" placeholder="Confirm Password" class="auth-input" />
                  <span class="toggle-password" @click="showConfirm = !showConfirm">
                    <i :class="showConfirm ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                  </span>
                </div>
              </div>

              <div class="password-rules-container">
                <p class="password-title">Your Password Must Have:</p>
                <ul class="password-rules">
                  <li><i :class="['fa', hasUppercase ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> One uppercase letter</li>
                  <li><i :class="['fa', hasLowercase ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> One lowercase letter</li>
                  <li><i :class="['fa', hasNumber ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> At least one number</li>
                  <li><i :class="['fa', noSpaces ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> No spaces</li>
                  <li><i :class="['fa', minLength ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> 8 or more characters</li>
                </ul>
              </div>
              <div class="captcha-wrapper">
                <Captcha :key="captchaKey" @verified="onCaptchaVerified" />
              </div>
            </div>

            <div class="register-grid-container">
              <!--<div>
                <p class="auth-input-label">Government ID Type</p>
                <select v-model="govIDType" class="auth-input">
                  <option value="" disabled>-- Select ID Type --</option>
                  <option value="UMID">UMID</option>
                  <option value="Drivers License">Driver's License</option>
                  <option value="Passport">Passport</option>
                  <option value="PhilSys">PhilSys (National ID)</option>
                </select>
              </div>
              <div>
                <p class="auth-input-label">Government ID Number</p>
                <input v-model="govIDNumber" type="text" placeholder="Enter ID Number" class="auth-input" />
              </div>-->

              <div style="grid-column: 1 / -1;">
                <p class="auth-input-label">Upload Valid ID Image*</p>
                <div class="file-action-container" style="border: 2px dashed #e0e0e0; padding: 15px; border-radius: 10px;">
                  <div v-if="!idImageBase64" class="upload-file-btn" @click="triggerValidIDUpload" style="cursor: pointer; text-align: center;">
                    <i class="fas fa-camera"></i> <strong>Click to Upload Image</strong>
                  </div>
                  <div v-else class="uploaded-file-details" style="display: flex; justify-content: space-between; align-items: center;">
                    <span style="color: #2e7d32; font-weight: 600;">✅ {{ validIDOriginalName }}</span>
                    <button @click="removeID" style="color: #dc2626; border: none; background: none; cursor: pointer;">Remove</button>
                  </div>
                </div>
              </div>

              <input type="file" ref="requiredDocFile" @change="handleGenericFileSelection" style="display: none;" accept="image/*" />
            </div>

            <div class="terms-captcha-row">
              <div class="terms-wrapper">
                <input type="checkbox" v-model="isTermsAccepted" />
                I accept the
                <span class="link" @click="showTerms = true">Terms of Service</span>
              </div>
            </div>

            <TermsOfService :show="showTerms"
                            @update:show="showTerms = $event"
                            @accepted="isTermsAccepted = true" />

            <p v-if="passwordMismatch" class="error-text">Passwords do not match</p>
            <button @click="handleRegister" class="auth-btn" :disabled="!canRegister || isLoading">Sign Up</button>

            <p class="text-center">
              Already have an account?
              <span class="link" @click="isLogin = true">Sign In</span>
            </p>
          </div>
        </div>
      </div>
    </div>


    <DialogBox :show="showDialog" :title="dialogTitle" :message="dialogMessage" @close="showDialog = false" />



    <DialogBox :show="showForgotDialog" title="Reset Password" @close="showForgotDialog = false">
      <div style="display:flex; flex-direction:column; gap:8px; margin-top:8px;">
        <p class="auth-input-label">Enter your email address</p>
        <input v-model="forgotEmail" type="email" placeholder="Enter your email" class="auth-input" />
        <button @click="handleForgotPassword" class="auth-btn" :disabled="isLoading">Send Reset Link</button>
      </div>
    </DialogBox>



    <DialogBox :show="showOtpDialog"
               title="OTP Verification"
               message="Enter the 6-digit OTP sent to your email to verify your login."
               @close="showOtpDialog = false">

      <div class="otp-container">

        <!-- OTP INPUTS -->
        <div class="otp-input-group">
          <input v-for="(digit, index) in otpArray"
                 :key="index"
                 :id="'otp-' + index"
                 v-model="otpArray[index]"
                 type="text"
                 maxlength="1"
                 class="otp-box"
                 @input="handleOtpInput($event, index)"
                 @keydown.delete="handleOtpDelete($event, index)"
                 @paste="handleOtpPaste" />
        </div>

        <!-- ERROR -->
        <p v-if="otpError" class="error-text">{{ otpError }}</p>

        <!-- SUBMIT -->
        <button @click="handleVerifyOtp"
                class="auth-btn"
                :disabled="otpArray.join('').length < 6 || isLoading">
          {{ isLoading ? "Verifying..." : "Submit" }}
        </button>

        <!-- 🔥 CIRCULAR TIMER -->
        <div v-if="!showResend && resendAttempts < maxAttempts"
             class="timer-container">

          <svg width="120" height="120">
            <!-- Background circle -->
            <circle cx="60"
                    cy="60"
                    :r="radius"
                    stroke="#eee"
                    stroke-width="8"
                    fill="none" />

            <!-- Animated progress -->
            <circle cx="60"
                    cy="60"
                    :r="radius"
                    stroke="#42b883"
                    stroke-width="8"
                    fill="none"
                    :stroke-dasharray="circumference"
                    :stroke-dashoffset="strokeDashoffset"
                    stroke-linecap="round"
                    transform="rotate(-90 60 60)"
                    style="transition: stroke-dashoffset 1s linear;" />
          </svg>

          <p class="cooldown-text">
            Resend available in {{ countdown }}s
          </p>
        </div>

        <!-- 🔥 RESEND BUTTON -->
        <button v-if="showResend && resendAttempts < maxAttempts"
                @click="handleResendOtp"
                class="auth-btn resend-btn"
                :disabled="resendLoading">
          {{
 resendLoading
          ? "Resending..."
          : `Resend OTP (${maxAttempts - resendAttempts} left)`
          }}
        </button>

        <!-- 🔥 MAX ATTEMPTS REACHED -->
        <p v-if="resendAttempts >= maxAttempts"
           class="error-text">
          You have reached the maximum resend attempts (3).
        </p>

      </div>
    </DialogBox>




    <LoadingDialog :visible="isLoading" />
  </div>
</template>

<script setup>
  // NOTE: Assuming these components are available in the project structure
  import Captcha from '@/components/Auth/Captcha.vue'
  import { ref, computed, watch, nextTick, onMounted, onUnmounted } from "vue";
  import { useRouter, useRoute } from "vue-router";
  import api from "@/api";
  import { useAuthStore } from "@/stores/auth";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import TermsOfService from "@/components/TermsOfService.vue";

  // Using @vueuse/head for script injection
  import { useHead } from '@vueuse/head';



  useHead({
    script: [
      {
        src: 'https://www.google.com/recaptcha/api.js',
        async: true,
        defer: true,
      },
    ],
  });

  function onSubmit(token) {
    document.getElementById("demo-form").submit();
  }

  // Router & Auth
  const router = useRouter();
  const route = useRoute();
  const auth = useAuthStore();

  // captcha verification
  const captchaVerified = ref(false);
  const captchaKey = ref(0);

  // terms of service
  const showTerms = ref(false);
  const isTermsAccepted = ref(false);

  // Login/Register State
  const isLogin = ref(true);
  const loginStep = ref("credentials");
  const username = ref("citizen.jane@email.com"); // Dont forget to leave empty
  const email = ref("");
  const password = ref("Pssic123@"); // Dont forget to leave empty
  const confirmPassword = ref("");
  const otp = ref("");
  const showPassword = ref(true); // Dont forget to leave to false
  const showConfirm = ref(false);

  // OTP animation and countdown reference

  const cooldownSeconds = 60;
  const countdown = ref(10);
  const resendAttempts = ref(0);
  const maxAttempts = 3;
  const radius = 45;
  const circumference = 2 * Math.PI * radius;

  const strokeDashoffset = computed(() => {
    return circumference - (countdown.value / cooldownSeconds) * circumference;
  });

  let timer = null;

  // Role selection
  // const userRole = ref("Employee"); // <--- REMOVED: Replaced by isEmployee

  const employeeID = ref("");
  const lastName = ref("");
  const firstName = ref("");
  const birthday = ref("");

  // 🔥 NEW: Employee registration state
  const isEmployee = ref(false);

  // Dialogs
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const showForgotDialog = ref(false);
  const showOtpDialog = ref(false);
  const forgotEmail = ref("");
  const otpError = ref("");
  const isLoading = ref(false);
  const resendLoading = ref(false);
  const showResend = ref(false);

  const otpArray = ref(['', '', '', '', '', '']);

  // Computed
  const passwordMismatch = computed(() => confirmPassword.value && password.value !== confirmPassword.value);
  const canLogin = computed(() => username.value && password.value);

  // 🔥 UPDATED: Checks that employeeID is filled IF isEmployee is true.
  const isEmployeeIdRequired = computed(() => !isEmployee.value || !!employeeID.value);

  // 🔥 UPDATED: Check if all registration fields are valid/provided.
  const canRegister = computed(() => {
    const isNameOrIdValid = isEmployee.value ? !!employeeID.value : !!firstName.value;

    const baseValid = (
      isNameOrIdValid && // Check that either employeeID or firstName is filled
      lastName.value &&
      birthday.value &&
      email.value &&
      isPasswordValid.value &&
      !passwordMismatch.value &&
      //govIDType.value &&
      //govIDNumber.value &&
      idImageBase64.value && // Ensure ID image is captured
      isTermsAccepted.value &&
      captchaVerified.value
    );
    return baseValid;
  });


  // password validation
  // Individual checks
  const hasUppercase = computed(() => /[A-Z]/.test(password.value));
  const hasLowercase = computed(() => /[a-z]/.test(password.value));
  const hasNumber = computed(() => /[0-9]/.test(password.value));
  const noSpaces = computed(() => !/\s/.test(password.value));
  const minLength = computed(() => password.value.length >= 8);

  const isPasswordValid = computed(() =>
    hasUppercase.value &&
    hasLowercase.value &&
    hasNumber.value &&
    noSpaces.value &&
    minLength.value
  );

  const validatePassword = () => {
    // UI update handled by computed properties
  };


  // Forgot Password
  const openForgotPasswordDialog = () => {
    forgotEmail.value = username.value || "";
    showForgotDialog.value = true;
  };


  const handleForgotPassword = async () => {
    if (!forgotEmail.value) return;
    try {
      isLoading.value = true;
      await api.post("/auth/forgot-password", { email: forgotEmail.value });
      dialogTitle.value = "Success";
      dialogMessage.value = "✅ Check your email for password reset instructions.";
      showDialog.value = true;
    } catch (err) {
      dialogTitle.value = "Error";
      dialogMessage.value = err.response?.data?.message || "Something went wrong.";
      showDialog.value = true;
    } finally {
      isLoading.value = false;
      showForgotDialog.value = false;
    }
  };

  // --- New Gov ID State ---
  const govIDType = ref("");
  const govIDNumber = ref("");
  const idImageBase64 = ref("");
  const idFileExtension = ref("");
  const validIDOriginalName = ref(null);
  const documentTypeToUpload = ref(""); // Needed for trigger logic
  const requiredDocFile = ref(null);    // Needed for input ref


  watch(isLogin, (newVal) => {
    if (newVal) {
      // Switched to Login
      username.value = "";
      password.value = "";
      otp.value = "";
      forgotEmail.value = "";
      otpError.value = "";
    } else {
      // Switched to Registration
      resetRegistrationForm();
    }
  });

  const onCaptchaVerified = (status) => {
    captchaVerified.value = status;
  };


  const resetRegistrationForm = () => {
    employeeID.value = "";
    birthday.value = "";
    firstName.value = "";
    lastName.value = "";
    email.value = "";
    password.value = "";
    confirmPassword.value = "";
    isEmployee.value = false; // 🔥 UPDATED: Reset employee state
    govIDType.value = "";
    govIDNumber.value = "";
    removeID();
    isTermsAccepted.value = false;
    captchaVerified.value = false;
    showTerms.value = false;
    captchaKey.value++; // refresh CAPTCHA
  };

  // --- New Error State ---
  const fieldErrors = ref({
    email: false,
    employeeID: false,
    firstName: false,
    lastName: false,
    password: false,
    birthday: false // Added for BirthDate validation
  });

  const clearErrors = () => {
    fieldErrors.value = {
      email: false,
      employeeID: false,
      firstName: false,
      lastName: false,
      password: false,
      birthday: false
    };
  };

  // Register
  const handleRegister = async () => {
    clearErrors();

    // 1. FRONT-END VALIDATION: Check required fields upfront
    if (!email.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "Email Address is required.";
      showDialog.value = true;
      return;
    }

    // 🔥 UPDATED: Check for the relevant ID/Name field
    if (isEmployee.value && !employeeID.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "Employee ID is required for employee registration.";
      showDialog.value = true;
      return;
    } else if (!isEmployee.value && !firstName.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "First Name is required for citizen registration.";
      showDialog.value = true;
      return;
    }

    if (!isPasswordValid.value) {
      dialogTitle.value = "Password";
      dialogMessage.value = "Please ensure your password meets all the requirements.";
      showDialog.value = true;
      return;
    }

    if (passwordMismatch.value) {
      dialogTitle.value = "Password Mismatch";
      dialogMessage.value = "The confirmed password does not match the password.";
      showDialog.value = true;
      return;
    }


    if (!isTermsAccepted.value) {
      dialogTitle.value = "Terms of Service";
      dialogMessage.value = "Please accept the Terms of Service before registering.";
      showDialog.value = true;
      return;
    }


    if (!captchaVerified.value) {
      dialogTitle.value = "Captcha Required";
      dialogMessage.value = "Please verify the captcha before signing up.";
      showDialog.value = true;
      return;
    }


    try {
      isLoading.value = true;

      // 2. DATA PAYLOAD PREPARATION
      const registrationData = {
        username: email.value,
        email: email.value,
        password: password.value,
        firstName: firstName.value,
        lastName: lastName.value,
        employeeID: isEmployee.value ? employeeID.value : "",
        birthDate: birthday.value ? new Date(birthday.value).toISOString() : null,
        userRole: isEmployee.value ? 1 : 0,
        govIDType: govIDType.value,
        govIDNumber: govIDNumber.value,
        idImageBase64: idImageBase64.value,
        idFileExtension: idFileExtension.value
      };


      const res = await api.post("/auth/register", registrationData);

      dialogTitle.value = "Success";
      dialogMessage.value = res.data.message || "Registration successful ✅";
      showDialog.value = true;

      isLogin.value = true;
      resetRegistrationForm();
      username.value = email.value;
    } catch (err) {
      console.error("FULL REGISTRATION ERROR:", err);
      dialogTitle.value = "Registration Failed";

      let msg = "Registration failed.";
      if (err.response?.data?.message) {
    msg = err.response.data.message;
    const lowerMsg = msg.toLowerCase();

    // 1. Check for Birthdate first (Specific mismatch)
    if (lowerMsg.includes("birth date") || lowerMsg.includes("birthdate")) {
      fieldErrors.value.birthday = true;
    }
    // 2. Check for Name mismatches
    else if (lowerMsg.includes("first name")) {
      fieldErrors.value.firstName = true;
    }
    else if (lowerMsg.includes("last name") || lowerMsg.includes("surname")) {
      fieldErrors.value.lastName = true;
    }
    // 3. Check for Email
    else if (lowerMsg.includes("email")) {
      fieldErrors.value.email = true;
    }
    // 4. Check for Employee ID (General catch-all for ID errors)
    else if (lowerMsg.includes("employee id")) {
      fieldErrors.value.employeeID = true;
    }

  } else if (err.response?.data?.errors) {
    // Handle ASP.NET ModelState errors (Dictionary based)
    const errorData = err.response.data.errors;
    msg = Object.values(errorData).flat().join("\n");

    if (errorData.Email) fieldErrors.value.email = true;
    if (errorData.EmployeeID) fieldErrors.value.employeeID = true;
    if (errorData.FirstName) fieldErrors.value.firstName = true;
    if (errorData.LastName || errorData.Surname) fieldErrors.value.lastName = true;
    if (errorData.BirthDate) fieldErrors.value.birthday = true;
  }
      captchaKey.value++; // refresh CAPTCHA
      dialogMessage.value = msg;
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  };

  // Login Step 1: credentials → send OTP
  // AuthPage.vue: Inside handleLogin function
  const handleLogin = async () => {
    try {
      isLoading.value = true;
      otpArray.value = ['', '', '', '', '', ''];
      await api.post("/auth/login", {
        email: username.value, // Make sure this key is 'email'
        password: password.value,
      });
      showOtpDialog.value = true;
      startCooldown(); // 🔥 start 60 second timer

    } catch (err) {
      dialogTitle.value = "Login Failed";

      // 1. Initialize 'msg' with a strong fallback string
      let msg = "Login failed. A connection or unknown error occurred.";

      if (err.response?.data?.message) {
        msg = err.response.data.message;
      } else if (err.response?.data?.errors) {
        // Handle ASP.NET ModelState errors
        const errors = Object.values(err.response.data.errors).flat();
        msg = errors.join("\n");
      } else if (err.message) {
        // Handle general network errors (e.g., Axios timeout)
        msg = err.message;
      }

      // 2. CRITICAL STEP: Explicitly ensure a string is assigned
      dialogMessage.value = String(msg);
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  };


  watch(otp, (newVal) => {
    if (!newVal) {
      otpError.value = ""; // clear error if otp is empty
    }
  });

  onMounted(() => {
  const endTime = localStorage.getItem("otpCooldownEnd");

  if (endTime) {
    const remaining = Math.floor((endTime - Date.now()) / 1000);

    if (remaining > 0) {
      countdown.value = remaining;
      showResend.value = false;
      timer = setInterval(updateCountdown, 1000);
    } else {
      localStorage.removeItem("otpCooldownEnd");
      showResend.value = true;
    }
  }
});

  onUnmounted(() => {
    if (timer) clearInterval(timer);
  });

  // OTP animation for countdown 1//

    // Show resend after 60 seconds
  const startCooldown = () => {
  showResend.value = false;
  countdown.value = 60;

  if (timer) clearInterval(timer);

   timer = setInterval(() => {
      countdown.value--;

      if (countdown.value <= 0) {
        clearInterval(timer);
        showResend.value = true; // 🔥 button appears after 60s
      }
    }, 1000);
  };

  const updateCountdown = () => {
  const endTime = localStorage.getItem("otpCooldownEnd");
  if (!endTime) return;

  const remaining = Math.floor((endTime - Date.now()) / 1000);

  if (remaining <= 0) {
    clearInterval(timer);
    localStorage.removeItem("otpCooldownEnd");
    showResend.value = true;
    countdown.value = 0;
  } else {
    countdown.value = remaining;
  }
};

  const handleVerifyOtp = async () => {
  const fullOtp = otpArray.value.join('');

  if (!fullOtp) return;

  if (fullOtp.length !== 6) {
    otpError.value = "OTP must be 6 digits";
    return;
  }

  isLoading.value = true;
  otpError.value = ""; // clear previous errors
  try {
    const res = await api.post("/auth/verify-otp", {
      email: username.value,
      otp: fullOtp,
    });

    auth.login({ token: res.data.token });
    showOtpDialog.value = false;

    // Role-based redirection
    const role = parseInt(auth.userRole);
    if (role === 4 || role === 5 || role === 6) {
      router.push("/dashboard");
    } else if (role === 1) {
      router.push("/dashboard-admin");
    } else {
      router.push("/");
    }

  } catch (err) {
    // Set the error message returned from backend
    otpError.value =
      err.response?.data?.message ||
      err.response?.data?.error ||
      err.message ||
      "Invalid or expired OTP";

      console.log (err);
  } finally {
    isLoading.value = false;
  }
};

  const handleResendOtp = async () => {
  if (resendAttempts.value >= maxAttempts) {
    otpError.value = "Maximum resend attempts reached.";
    return;
  }

  resendLoading.value = true;
  otpError.value = "";

  try {
    await api.post("/auth/resend-otp", {
      email: username.value
    });

    resendAttempts.value++;
    startCooldown();

  } catch (err) {
    otpError.value =
      err.response?.data?.message || "Failed to resend OTP";
  } finally {
    resendLoading.value = false;
  }
};



  // Handle typing in a box
  const handleOtpInput = (event, index) => {
    const value = event.target.value;
    // Ensure only numbers
    if (!/^\d$/.test(value)) {
      otpArray.value[index] = '';
      return;
    }

    // Move to next input
    if (value && index < 5) {
      const nextInput = document.getElementById(`otp-${index + 1}`);
      nextInput?.focus();
    }
  };

  // Handle backspace
  const handleOtpDelete = (event, index) => {
    if (!otpArray.value[index] && index > 0) {
      const prevInput = document.getElementById(`otp-${index - 1}`);
      prevInput?.focus();
    }
  };

  // Handle pasting the whole code
  const handleOtpPaste = (event) => {
    const pasteData = event.clipboardData.getData('text').slice(0, 6);
    if (!/^\d+$/.test(pasteData)) return;

    const digits = pasteData.split('');
    digits.forEach((d, i) => {
      if (i < 6) otpArray.value[i] = d;
    });

    // Focus the last filled box or the 6th box
    const targetIndex = digits.length === 6 ? 5 : digits.length;
    document.getElementById(`otp-${targetIndex}`)?.focus();
  };

  // --- File Methods ---
  function triggerValidIDUpload() {
    documentTypeToUpload.value = 'ValidID';
    if (requiredDocFile.value) requiredDocFile.value.click();
  }

  // ------------------ Upload Handler ------------------
  /**
 * Reads the file, extracts extension and converts to Base64
 */
  async function handleGenericFileSelection(event) {
    const file = event.target.files[0];
    if (!file) return;

    idFileExtension.value = file.name.substring(file.name.lastIndexOf('.'));
    validIDOriginalName.value = file.name;

    const reader = new FileReader();
    reader.onload = (e) => {
      idImageBase64.value = e.target.result.split(',')[1];
    };
    reader.readAsDataURL(file);
    event.target.value = null;
  }

  function removeID() {
    idImageBase64.value = "";
    idFileExtension.value = "";
    validIDOriginalName.value = null;
  }

  // ------------------ Delete Logic ------------------
  async function deleteFile(docType, fileKey) {
    if (!fileKey) return;

    isLoading.value = true;
    dialogTitle.value = "Deleting";
    showDialog.value = true;

    try {
      // Registration files are always temporary until the user is created
      await api.delete(`/employee/deleteupload?fileKey=${fileKey}`);

      if (docType === 'ValidID') {
        validIDFileKey.value = null;
        validIDOriginalName.value = null;
      }

      dialogTitle.value = "Success";
      dialogMessage.value = "ID removed successfully.";
    } catch (err) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to remove the file.";
    } finally {
      isLoading.value = false;
    }
  }
</script>

<style scoped>
  /* Import Font Awesome for the icons */
  @import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css');
  /*
  * AUTH.CSS - ADAPTED FOR PERFECTLY CENTERED TWO-COLUMN LAYOUT
  * Theme: Deep Blue (#004085)
  */

  /* --- PAGE WRAPPER --- */
  .auth-page-wrapper {
    display: flex;
    justify-content: center; /* Center horizontally */
    align-items: center; /* Center vertically */
    min-height: 100vh;
    /* Use padding only for content, not for scroll management */
    padding: 20px;
    background: linear-gradient(135deg, #eaf3ff, #f9ffff);
    font-family: 'Inter', sans-serif;
    box-sizing: border-box;
    /* Allow scroll on the whole page wrapper if content exceeds viewport */
    overflow-y: auto;
    /* FIX 1: Set a dark base text color for the entire page wrapper */
    color: #333333; /* Dark gray for general text */
  }




    /*
   * NEW: Adjust vertical alignment when the register form is active
   * This prevents the taller register form from being perfectly centered on its height,
   * which often pushes the top of the form outside the viewport. By setting align-items
   * to flex-start, the content starts from the top, respecting the 20px padding.
  */
    .auth-page-wrapper.register-active {
      align-items: flex-start;
    }




  /* --- MAIN LAYOUT CONTAINER (for desktop split) --- */
  .auth-main-layout {
    display: flex;
    width: 100%;
    /* INCREASED: Increase overall max-width for more breathing room */
    max-width: 1250px;
    align-items: center;
    gap: 60px;
    flex-shrink: 0;
    flex-grow: 0;
    min-height: fit-content;
    padding: 0;
  }




  /* --- LOGO AND TITLE SECTION (Left Side on Desktop) --- */
  .logo-container {
    text-align: left;
    /* INCREASED: Increase max-width for symmetry with auth-container */
    flex: 1;
    max-width: 1000px;
    min-width: 300px;
    align-self: center;
  }

  .page-title h2 {
    font-size: 1.4rem;
    color: #64748b;
    font-weight: 500;
  }

  .page-title h1 {
    font-size: 3.2rem;
    font-weight: 800;
    color: #002a57;
    margin-top: -5px;
  }




  /* --- AUTH CONTAINER/BOX (Right Side on Desktop - The "Card") --- */
  .auth-container {
    /* INCREASED: Increase max-width to make the auth box wider */
    flex: 1;
    min-width: 400px;
    max-width: 580px;
    flex-shrink: 0;
    padding: 0;
    margin-top: 0;
    margin-bottom: 0;
    align-self: center;
  }

  .auth-box {
    background: white;
    border-radius: 20px;
    box-shadow: 0 15px 40px rgba(0, 42, 87, 0.25), 0 5px 15px rgba(0, 0, 0, 0.1);
    padding: 25px;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
    border: 1px solid #e0e0e0;
    /* NEW: Prevent auth-box content from overflowing by giving it a max-height */
    /* This uses calc() to reserve space for the page padding (40px) */
    max-height: calc(100vh - 40px);
    overflow-y: auto; /* Allow scrolling inside the box if it gets too tall */
    /* FIX 2: Ensure all text inside auth-box is readable against its white background */
    color: #333333;
  }




    /* Optional: slight lift on hover */
    .auth-box:hover {
      transform: translateY(-3px);
      box-shadow: 0 20px 50px rgba(0, 42, 87, 0.3), 0 5px 15px rgba(0, 0, 0, 0.15);
    }

  .auth-title {
    font-size: 2.4rem;
    font-weight: 800;
    margin-bottom: 30px;
    margin-top: 5px;
    color: #002a57;
    text-align: center;
  }




  /* ------------------- FORM ELEMENTS ------------------- */
  .auth-input-label {
    font-size: 0.9em;
    font-weight: 600;
    color: #002a57;
    margin-top: 0;
    margin-bottom: 4px; /* Reduced margin */
  }

  .auth-input {
    width: 100%;
    /* Standard padding for all text/date inputs */
    padding: 12px 15px;
    border: 1px solid #cbd5e1;
    border-radius: 8px;
    font-size: 1.05rem;
    transition: border-color 0.3s ease, box-shadow 0.3s ease;
    font-family: 'Inter', sans-serif;
    box-sizing: border-box;
    /* FIX 3: Ensure input text is dark */
    color: #333;
    /* FIX 4: Ensure input placeholder text is visible, especially in dark mode */
    background-color: white;
    /* Ensure regular inputs don't have hidden browser controls or custom arrows */
    appearance: initial;
    -webkit-appearance: initial;
    -moz-appearance: initial;
    background-image: none;
  }

  .auth-input-group {
    /* This creates the space between the Email Address group and the Password group */
    margin-bottom: 20px;
  }

  /* ------------------------------------------------------------------- */
  /* NEW: Specific styling for SELECT elements (Register as) */
  /* ------------------------------------------------------------------- */
  .auth-input select {
    /* Apply custom appearance and extra padding only to SELECT */
    appearance: none;
    -webkit-appearance: none;
    -moz-appearance: none;
    /* INCREASED RIGHT PADDING: Now applied only to the SELECT field with the arrow */
    padding-right: 35px;
    /* Custom arrow using SVG, matching your brand color (#002a57) */
    background-image: url('data:image/svg+xml;charset=US-ASCII,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%22292.4%22%20height%3D%22292.4%22%3E%3Cpath%20fill%3D%22%23002a57%22%20d%3D%22M287%2069.4l-14.9-14.9c-2.3-2.3-5.3-3.6-8.5-3.6s-6.2%201.3-8.5%203.6L146.2%20174.4%2050.4%2078.6c-2.3-2.3-5.3-3.6-8.5-3.6s-6.2%201.3-8.5%203.6L18.4%2093.5c-4.7%204.7-4.7%2012.3%200%2017l128%20128c4.7%204.7%2012.3%204.7%2017%200l128-128c4.7-4.7%204.7-12.3%200-17z%22%2F%3E%3C%2Fsvg%3E');
    background-repeat: no-repeat;
    background-position: right 10px center; /* Position the arrow */
    background-size: 10px; /* Size the arrow */
  }


  /* ------------------------------------------------------------------- */

  /* END: Specific styling for SELECT elements */

  /* ------------------------------------------------------------------- */
  .auth-input:focus {
    outline: none;
    border-color: #004085;
    box-shadow: 0 0 8px rgba(0, 64, 133, 0.4);
  }



  /* HIDE BROWSER-PROVIDED SHOW/HIDE PASSWORD TOGGLE */
  .auth-input[type="password"]::-ms-reveal,
  .auth-input[type="password"]::-webkit-reveal {
    display: none !important;
  }



  /* FIX 5: Ensure placeholder color is not white in dark mode */
  .auth-input::placeholder {
    color: #94a3b8; /* A light gray for visibility */
  }

  .password-wrapper {
    position: relative;
  }

  .toggle-password {
    position: absolute;
    top: 50%;
    right: 15px;
    transform: translateY(-50%);
    cursor: pointer;
    color: #64748b;
    padding: 5px;
  }




  /* ------------------- BUTTONS & LINKS ------------------- */
  .auth-btn {
    width: 100%;
    padding: 16px 15px; /* Reduced padding for smaller height */
    border: none;
    border-radius: 12px;
    font-size: 1.1rem;
    font-weight: 700;
    color: white;
    background: linear-gradient(180deg, #4a698d, #3b506b);
    cursor: pointer;
    transition: background-color 0.3s ease, transform 0.1s, box-shadow 0.3s ease;
    margin-top: 25px; /* Reduced margin */
    margin-bottom: 15px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
    font-family: 'Inter', sans-serif;
    box-sizing: border-box;
  }

    .auth-btn:hover:not(:disabled) {
      background: #3b506b;
      transform: translateY(-1px);
      box-shadow: 0 6px 18px rgba(0, 0, 0, 0.3);
    }

    .auth-btn:disabled {
      background: #b3cde0;
      box-shadow: none;
      cursor: not-allowed;
    }

  .forgot-password {
    text-align: right;
    margin-top: 8px; /* Reduced margin */
    margin-bottom: 15px; /* Reduced margin */
  }

  .link, .forgot-password {
    color: #004085;
    text-decoration: none;
    font-weight: 600;
    transition: color 0.2s;
    cursor: pointer;
  }

    .link:hover, .forgot-password:hover {
      color: #002a57;
      text-decoration: underline;
    }



  /* FIX 6: Explicitly set color for the regular text paragraphs that were likely affected */
  .text-center {
    /* Inherits from .auth-box, but setting it here for explicit control */
    color: #333333;
  }




  /* ------------------- REGISTRATION SPECIFIC ------------------- */
  .register-grid-container {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px 15px; /* Significantly reduced vertical gap from 25px to 15px */
  }

  .password-rules-container {
    padding: 0;
    margin-top: 15px; /* Reduced margin */
  }

  .password-title {
    font-weight: 600;
    color: #002a57;
    font-size: 0.9em;
    margin-bottom: 5px;
    margin-top: 0;
  }

  .password-rules {
    list-style: none;
    padding: 0;
    margin: 0;
    /* FIX 7: Ensure list item text is readable */
    color: #333333;
  }

    .password-rules li {
      margin-bottom: 3px; /* Reduced margin */
      font-size: 0.8em; /* Reduced font size */
      display: flex;
      align-items: center;
      gap: 8px;
    }

  .valid {
    color: #388e3c;
  }

  .invalid {
    color: #d32f2f;
  }

  .terms-captcha-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 20px; /* Reduced margin */
    margin-bottom: 15px; /* Reduced margin */
    grid-column: 1 / -1;
  }

  .terms-wrapper {
    margin-top: 0;
    margin-bottom: 0;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.95em;
    color: #333;
  }

    .terms-wrapper input[type="checkbox"] {
      transform: scale(1.2);
      cursor: pointer;
      accent-color: #004085;
    }

  .error-text {
    color: #d32f2f;
    font-size: 0.9em;
    font-weight: 500;
    text-align: center;
    margin-top: 10px;
  }




  /* --- MEDIA QUERIES (Switch to mobile view) --- */
  @media (max-width: 960px) {
    .auth-main-layout {
      flex-direction: column;
      align-items: center; /* Center items when stacked */
      gap: 30px;
    }




    /* Reset the align-items rule when it collapses to a column */
    .auth-page-wrapper.register-active {
      align-items: center;
    }

    .auth-box {
      /* Allow the box to expand fully on smaller screens,
           relying on the main wrapper's scroll (or mobile's default scroll) */
      max-height: none;
    }

    .logo-container {
      text-align: center;
      max-width: 100%;
      min-width: unset;
      /* Reset max-width on mobile to allow full width usage */
      max-width: 100%;
    }

    .page-title h1 {
      font-size: 2.2rem;
    }

    .auth-container {
      max-width: 100%;
      min-width: 100%;
      /* Reset max-width on mobile to allow full width usage */
      max-width: 100%;
    }

    .terms-captcha-row {
      flex-direction: column;
      align-items: flex-start;
      gap: 15px;
    }
  }

  @media (max-width: 600px) {
    .register-grid-container {
      grid-template-columns: 1fr;
    }

    .password-rules-container {
      margin-top: 15px;
    }
  }

  /* Container for the whole OTP section */
  .otp-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 100%;
  }

  /* Force items into a single row */
  .otp-input-group {
    display: flex;
    flex-direction: row; /* Ensures horizontal alignment */
    justify-content: center;
    align-items: center;
    gap: 10px; /* Space between boxes */
    margin: 25px 0;
    width: 100%;
  }

  /* Individual Boxes */
  .otp-box {
    width: 45px;
    height: 55px;
    text-align: center;
    font-size: 1.4rem;
    font-weight: 800;
    border: 2px solid #ced4da;
    border-radius: 8px;
    background: #ffffff;
    color: #002a57;
    transition: all 0.2s ease-in-out;
    /* Prevent zooming on mobile */
    line-height: normal;
  }

    .otp-box:focus {
      border-color: #004085;
      background: #f0f7ff;
      box-shadow: 0 0 0 3px rgba(0, 64, 133, 0.1);
      outline: none;
    }

  /* Ensure the button stays centered below the row */
  .otp-container .auth-btn {
    margin-top: 10px;
    width: 100%;
    max-width: 300px;
  }

  .error-border {
    border: 2px solid #dc2626 !important; /* Makati Red / Danger Red */
    background-color: #fef2f2 !important; /* Very light red tint */
    transition: all 0.3s ease;
  }

  /* Optional: add a shake animation for better UX */
  .error-border {
    animation: shake 0.4s focus;
  }

  @keyframes shake {
    0%, 100% {
      transform: translateX(0);
    }

    25% {
      transform: translateX(-5px);
    }

    75% {
      transform: translateX(5px);
    }
  }
</style>
