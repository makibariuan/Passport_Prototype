<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />
    <Header title="Application" class="header" />

    <div class="dashboard-content">
      <h2 class="page-title">Application for</h2>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 0 — Terms & Conditions                           -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-if="preStep === 'terms'" class="pre-step-wrapper">
        <div class="terms-card">
          <h2 class="terms-title">
            Welcome to the<br /><strong>DFA Online Appointment System</strong>
          </h2>
          <p class="terms-subtitle">
            Review all fields in the online form carefully and provide complete and accurate
            information
          </p>

          <div class="reminder-box">
            <span class="reminder-label">Reminder</span>
            <p>
              Applicants are advised to use only Google or Yahoo email accounts in securing an
              appointment to avoid any technical incompatibilities. Email address restrictions and
              accessibility may vary on your access location, country and/or email servers.
            </p>
          </div>

          <h3 class="terms-section-title">Terms and conditions</h3>
          <p>
            This appointment and scheduling system allocates slots on a first come, first served
            basis.
          </p>
          <p>
            Users accept the responsibility for supplying, checking, and verifying the accuracy and
            correctness of the information they provide on this system in connection with their
            application. Incorrect or inaccurate information supplied may result in forfeiture of
            passport application.
          </p>
          <p>
            For sites utilizing the ePayment System, all fees are non-refundable. Fees shall be
            forfeited for applicants who fail to show up on their confirmed appointment, applicants
            who cancel their appointment, applicants whose application was rejected due to
            inconsistency and/or incorrect information, and applicants who present discrepant and/or
            spurious documents.
          </p>

          <div class="consent-row">
            <label class="consent-label">
              <input v-model="consentChecked" type="checkbox" class="consent-checkbox" />
              <span class="consent-text">
                By proceeding with this application, I understand that I am signifying my
                unequivocal consent to the disclosure, collection, and use of my personal
                information and the data required under the Philippine Passport Act as amended and
                its Implementing Rules and Regulations. My consent effectively constitutes a waiver
                of any and all privacy rights pertaining to the disclosure, collection, and use of
                my personal information and data under the specific terms and condition of the DFA
                Online Passport Appointment System Website's Privacy Policy, the Data Privacy Act of
                2012 and its Implementing Rules and Regulations and other pertinent DFA rules,
                regulations, policies on the matter.
              </span>
            </label>
          </div>

          <div class="terms-actions">
            <button class="btn btn-continue-existing" @click="continueExisting">
              ↻ Continue existing application
            </button>
            <div class="terms-new-actions">
              <button
                class="btn btn-individual"
                :disabled="!consentChecked"
                @click="startIndividual"
              >
                👤 Individual application
              </button>
              <div class="group-btn-wrap">
                <button class="btn btn-group" :disabled="!consentChecked" @click="startGroup">
                  👥 Group application
                </button>
                <p class="group-note">At least two profiles are needed for group appointment</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 1 — Select Profile Modal                         -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-else-if="preStep === 'selectProfile'" class="pre-step-wrapper">
        <!-- blurred background hint -->
        <div class="modal-overlay">
          <div class="modal-card">
            <div class="modal-icon">
              <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
                <circle cx="24" cy="18" r="8" stroke="#06195e" stroke-width="2.5" fill="none" />
                <path
                  d="M8 42c0-8.837 7.163-16 16-16s16 7.163 16 16"
                  stroke="#06195e"
                  stroke-width="2.5"
                  stroke-linecap="round"
                  fill="none"
                />
              </svg>
            </div>
            <h3 class="modal-title">Select Profile</h3>
            <p class="modal-subtitle">Select the profile of the applicant to continue</p>

            <div class="profile-list">
              <label
                v-for="profile in profiles"
                :key="profile.id"
                class="profile-option"
                :class="{ selected: selectedProfile === profile.id }"
              >
                <input
                  v-model="selectedProfile"
                  type="radio"
                  :value="profile.id"
                  class="profile-radio-input"
                />
                <svg class="profile-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <circle cx="10" cy="7" r="3.5" stroke="#06195e" stroke-width="1.5" fill="none" />
                  <path
                    d="M3 18c0-3.866 3.134-7 7-7s7 3.134 7 7"
                    stroke="#06195e"
                    stroke-width="1.5"
                    stroke-linecap="round"
                    fill="none"
                  />
                </svg>
                <span class="profile-name">{{ profile.name }}</span>
                <span class="profile-radio">
                  <span v-if="selectedProfile === profile.id" class="radio-dot" />
                </span>
              </label>
            </div>

            <button
              class="btn btn-proceed"
              :disabled="!selectedProfile"
              @click="proceedWithProfile"
            >
              Proceed
            </button>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 2 — Appointment Notice Modal                     -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-else-if="preStep === 'appointmentNotice'" class="pre-step-wrapper">
        <div class="modal-overlay">
          <div class="modal-card notice-card">
            <h3 class="modal-title">Appointment Notice</h3>
            <p class="modal-subtitle">
              You are attempting to secure an appointment through the Special Site Facility of the
              DFA.
            </p>

            <ol class="notice-list">
              <li>
                Appointment booked at any Temporary Off-Site Passport Service (TOPS) is
                non-transferable and cannot be rescheduled. If an applicant has an existing
                appointment at any TOPS or Consular Office, the existing TOPS or Consular Office
                appointment must first be cancelled. A new appointment can then be booked and paid.
              </li>
              <li>
                Applicants have a choice to either use a courier delivery service or pick-up their
                new passport. Applicants who wish to pick-up their passports will have to go to the
                Supervising Consular Office of the TOPS Site to personally claim their passports.
                Please click
                <a href="#" class="notice-link">here</a> for the requirements for passport release.
              </li>
              <li>
                Applicants who wish to have their passports released via courier must agree to have
                their current passport cancelled on-site.
              </li>
              <li>
                There is no expedited processing when you apply at a TOPS facility. Processing time
                is between 12–16 working days. The delivery of passports via courier will add 3–5
                working days to the passport turnaround time.
              </li>
              <li>
                Before you proceed, make sure that you have your current passport and credit/debit
                card details on hand if paying online. Only charges shown during the online booking
                process are to be paid. No other fee will be paid on-site, nor should applicants pay
                extra fees to obtain an appointment.
              </li>
            </ol>

            <div class="notice-actions">
              <button class="btn btn-cancel" @click="cancelNotice">Cancel</button>
              <button class="btn btn-agree" @click="agreeNotice">I Agree</button>
            </div>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- MAIN STEPPER — shown after all pre-steps are done         -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <template v-else>
        <div class="pds-top-bar">
          <div class="stepper">
            <div
              v-for="(tab, index) in tabs"
              :key="tab"
              :class="['step', { active: currentStep === index, done: currentStep > index }]"
            >
              <div class="step-circle">
                <span v-if="currentStep > index">✓</span>
                <span v-else>{{ index + 1 }}</span>
              </div>
              <div class="step-label">{{ tab }}</div>
            </div>
          </div>
        </div>

        <div class="tab-wrapper">
          <div class="tab-content">
            <transition name="fade-slide" mode="out-in">
              <div :key="currentStep" class="form-wrapper">
                <div v-if="currentStep === 0">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Site Location & Schedule</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Select your preferred DFA site and appointment schedule.
                    </p>
                  </div>
                </div>

                <div v-else-if="currentStep === 1">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Application Type</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Choose whether this is a new application, renewal, or replacement.
                    </p>
                  </div>
                </div>

                <div v-else-if="currentStep === 2">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Companions & Assistants</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Add any companions or assistants who will accompany you.
                    </p>
                  </div>
                </div>

                <div v-else-if="currentStep === 3">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Application Form</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Fill in your personal details for the passport application.
                    </p>
                  </div>
                </div>

                <div v-else-if="currentStep === 4">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Documentary Requirements</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Upload or confirm all required supporting documents.
                    </p>
                  </div>
                </div>

                <div v-else-if="currentStep === 5">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Payment</h3>
                    </div>
                    <p style="color: var(--color-text-secondary, #718096); font-size: 0.9rem">
                      Review your application summary and complete payment.
                    </p>
                  </div>
                </div>
              </div>
            </transition>
          </div>

          <div class="nav-buttons">
            <button class="btn btn-back" :disabled="currentStep === 0" @click="prevStep">
              ← Back
            </button>
            <div class="step-indicator">Step {{ currentStep + 1 }} of {{ tabs.length }}</div>
            <button v-if="currentStep < tabs.length - 1" class="btn btn-next" @click="nextStep">
              Next →
            </button>
            <button v-else class="btn btn-submit" @click="submit">Submit Application</button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import LeftMenu from "@/components/LeftMenu.vue";
import { ref } from "vue";

// ── Pre-step state ──────────────────────────────────────────────────
// 'terms' → 'selectProfile' → 'appointmentNotice' → null (main stepper)
const preStep = ref("terms");

const consentChecked = ref(false);

const profiles = ref([
  { id: 1, name: "MARVIN ALFRED MERCADO PICO" },
  // Add more profiles here as needed
]);
const selectedProfile = ref(null);

// ── Pre-step handlers ───────────────────────────────────────────────
const continueExisting = () => {
  // TODO: handle "continue existing application" routing
};

const startIndividual = () => {
  if (!consentChecked.value) return;
  preStep.value = "selectProfile";
};

const startGroup = () => {
  if (!consentChecked.value) return;
  preStep.value = "selectProfile";
};

const proceedWithProfile = () => {
  if (!selectedProfile.value) return;
  preStep.value = "appointmentNotice";
};

const cancelNotice = () => {
  // Go back to terms page
  preStep.value = "terms";
};

const agreeNotice = () => {
  // All pre-steps done — enter the main stepper
  preStep.value = null;
};

// ── Main stepper state ──────────────────────────────────────────────
const tabs = [
  "Site Location & Schedule",
  "Application Type",
  "Companions & Assistants",
  "Application Form",
  "Documentary Requirements",
  "Payment",
];

const currentStep = ref(0);

const nextStep = () => {
  if (currentStep.value < tabs.length - 1) currentStep.value++;
};

const prevStep = () => {
  if (currentStep.value > 0) currentStep.value--;
};

const submit = () => {
  // TODO: handle final submission
  alert("Application submitted!");
};
</script>

<style scoped>
/* ── Layout ──────────────────────────────────────────────────────── */
.app-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  grid-template-rows: auto 1fr;
  height: 100vh;
  background-color: #f4f7f9;
  overflow: hidden;
}
.leftMenu {
  grid-column: 1;
  grid-row: 1 / span 2;
  z-index: 100;
}
.header {
  grid-column: 2;
  grid-row: 1;
  z-index: 90;
}
.dashboard-content {
  grid-column: 2;
  grid-row: 2;
  padding: 30px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 20px;
}

/* ── Pre-step wrapper ────────────────────────────────────────────── */
.pre-step-wrapper {
  flex: 1;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  overflow-y: auto;
}

/* ── Terms card ──────────────────────────────────────────────────── */
.terms-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 36px 40px;
  max-width: 680px;
  width: 100%;
  box-shadow: 0 4px 16px rgba(6, 25, 94, 0.07);
}
.terms-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1a202c;
  text-align: center;
  margin: 0 0 6px;
  line-height: 1.5;
}
.terms-title strong {
  font-weight: 800;
}
.terms-subtitle {
  font-size: 0.82rem;
  color: #718096;
  text-align: center;
  margin: 0 0 20px;
}
.reminder-box {
  background: #fff8f0;
  border: 1px solid #f6ad55;
  border-radius: 8px;
  padding: 12px 16px;
  margin-bottom: 18px;
}
.reminder-label {
  font-size: 0.8rem;
  font-weight: 700;
  color: #c05621;
  display: block;
  margin-bottom: 4px;
}
.reminder-box p {
  margin: 0;
  font-size: 0.82rem;
  color: #744210;
}
.terms-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a202c;
  margin: 18px 0 8px;
}
.terms-card p {
  font-size: 0.82rem;
  color: #4a5568;
  line-height: 1.6;
  margin: 0 0 10px;
}

/* consent */
.consent-row {
  background: #f0f4ff;
  border: 1.5px solid #a3b4e8;
  border-radius: 8px;
  padding: 14px 16px;
  margin: 18px 0 22px;
}
.consent-label {
  display: flex;
  gap: 10px;
  cursor: pointer;
}
.consent-checkbox {
  flex-shrink: 0;
  width: 16px;
  height: 16px;
  margin-top: 2px;
  accent-color: #06195e;
  cursor: pointer;
}
.consent-text {
  font-size: 0.78rem;
  color: #2b4b9e;
  line-height: 1.6;
}

/* actions */
.terms-actions {
  display: flex;
  flex-direction: column;
  gap: 14px;
  align-items: center;
}
.terms-new-actions {
  display: flex;
  gap: 16px;
  align-items: flex-start;
  justify-content: center;
  flex-wrap: wrap;
}
.group-btn-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
}
.group-note {
  font-size: 0.72rem;
  color: #718096;
  margin: 4px 0 0;
  text-align: center;
}

/* ── Modal overlay ───────────────────────────────────────────────── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(30, 40, 80, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
  backdrop-filter: blur(2px);
}
.modal-card {
  background: #fff;
  border-radius: 14px;
  padding: 32px 36px;
  width: 420px;
  max-width: 92vw;
  box-shadow: 0 8px 40px rgba(6, 25, 94, 0.18);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}
.notice-card {
  width: 520px;
  align-items: flex-start;
}
.modal-icon {
  margin-bottom: 6px;
}
.modal-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0;
  text-align: center;
}
.notice-card .modal-title {
  text-align: left;
}
.modal-subtitle {
  font-size: 0.82rem;
  color: #718096;
  text-align: center;
  margin: 0 0 16px;
}
.notice-card .modal-subtitle {
  text-align: left;
}

/* profile list */
.profile-list {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 18px;
}
.profile-option {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  cursor: pointer;
  transition:
    border-color 0.15s,
    background 0.15s;
}
.profile-option.selected {
  border-color: #06195e;
  background: #f0f4ff;
}
.profile-radio-input {
  display: none;
}
.profile-icon {
  flex-shrink: 0;
}
.profile-name {
  flex: 1;
  font-size: 0.87rem;
  font-weight: 600;
  color: #1a202c;
}
.profile-radio {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid #06195e;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.radio-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: #06195e;
}

/* notice list */
.notice-list {
  padding-left: 20px;
  margin: 0 0 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.notice-list li {
  font-size: 0.82rem;
  color: #4a5568;
  line-height: 1.6;
}
.notice-link {
  color: #2b4b9e;
  text-decoration: underline;
}

/* notice actions */
.notice-actions {
  display: flex;
  gap: 12px;
  align-self: flex-end;
}

/* ── Buttons ─────────────────────────────────────────────────────── */
.btn {
  padding: 10px 24px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  border: none;
  transition: all 0.2s;
}
.btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.btn-continue-existing {
  background: #e67e22;
  color: #fff;
}
.btn-continue-existing:hover {
  background: #cf6d17;
}

.btn-individual {
  background: #06195e;
  color: #fff;
}
.btn-individual:not(:disabled):hover {
  background: #04134a;
}

.btn-group {
  background: #e2e8f0;
  color: #4a5568;
  border: 1.5px solid #cbd5e0;
}
.btn-group:not(:disabled):hover {
  background: #cbd5e0;
}

.btn-proceed {
  background: #06195e;
  color: #fff;
  width: 100%;
  margin-top: 4px;
  padding: 12px;
}
.btn-proceed:not(:disabled):hover {
  background: #04134a;
}
.btn-proceed:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.btn-cancel {
  background: #f1f5f9;
  color: #4a5568;
  border: 1.5px solid #d1d9e6;
}
.btn-cancel:hover {
  background: #e2e8f0;
}

.btn-agree {
  background: #06195e;
  color: #fff;
}
.btn-agree:hover {
  background: #04134a;
}

/* ── Stepper (unchanged from original) ──────────────────────────── */
.stepper {
  display: flex;
  align-items: flex-start;
  padding: 16px 0 0;
  overflow-x: auto;
}
.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex: 1;
  min-width: 80px;
  position: relative;
}
.step:not(:last-child)::after {
  content: "";
  position: absolute;
  top: 18px;
  left: calc(50% + 18px);
  right: calc(-50% + 18px);
  height: 2px;
  background: #d1d9e6;
  z-index: 0;
}
.step.done:not(:last-child)::after,
.step.active:not(:last-child)::after {
  background: #06195e;
}
.step-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 600;
  border: 2px solid #d1d9e6;
  background: #f4f7f9;
  color: #718096;
  position: relative;
  z-index: 1;
  transition: all 0.2s;
}
.step.active .step-circle {
  border-color: #06195e;
  background: #eef2fb;
  color: #06195e;
}
.step.done .step-circle {
  border-color: #06195e;
  background: #06195e;
  color: #fff;
}
.step-label {
  margin-top: 8px;
  font-size: 11px;
  color: #718096;
  text-align: center;
  line-height: 1.4;
  max-width: 90px;
}
.step.active .step-label {
  color: #06195e;
  font-weight: 600;
}
.step.done .step-label {
  color: #06195e;
}

/* ── Tab wrapper (unchanged) ─────────────────────────────────────── */
.tab-wrapper {
  background: white;
  border-radius: 0 12px 12px 12px;
  padding: 25px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  margin-top: 0;
}
.tab-content {
  flex: 1;
}
.form-wrapper {
  padding: 10px;
}
.pds-section {
  background: #fff;
  border: 1px solid #e8edf4;
  border-radius: 14px;
  padding: 24px 28px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(6, 25, 94, 0.05);
}
.pds-section-header {
  margin-bottom: 12px;
  padding-bottom: 12px;
  border-bottom: 2px solid #f0f4f9;
}
.pds-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0;
}

.nav-buttons {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid #e2e8f0;
}
.step-indicator {
  font-size: 0.82rem;
  color: #a0aec0;
  font-weight: 500;
}
.btn-back {
  background: #f1f5f9;
  color: #4a5568;
  border: 1.5px solid #d1d9e6;
}
.btn-back:not(:disabled):hover {
  background: #e2e8f0;
}
.btn-next {
  background: #06195e;
  color: #fff;
}
.btn-next:hover {
  background: #04134a;
}
.btn-submit {
  background: #276749;
  color: #fff;
}
.btn-submit:hover {
  background: #1e5038;
}

/* ── Transitions ─────────────────────────────────────────────────── */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition:
    opacity 0.25s ease,
    transform 0.25s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(16px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-16px);
}

/* ── Responsive ──────────────────────────────────────────────────── */
@media (max-width: 992px) {
  .app-layout {
    grid-template-columns: 80px 1fr;
  }
}
@media (max-width: 768px) {
  .app-layout {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto 1fr;
  }
  .leftMenu,
  .header,
  .dashboard-content {
    grid-column: 1;
  }
  .header {
    grid-row: 1;
  }
  .dashboard-content {
    grid-row: 2;
    padding: 20px 15px;
  }
  .leftMenu {
    grid-row: 3;
    width: 100%;
  }
  .step-label {
    display: none;
  }
  .nav-buttons {
    flex-wrap: wrap;
    gap: 12px;
  }
  .terms-card {
    padding: 24px 20px;
  }
  .modal-card {
    padding: 24px 20px;
  }
}
</style>
