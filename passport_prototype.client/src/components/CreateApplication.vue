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
              <button class="btn btn-individual"
                      :disabled="!consentChecked"
                      @click="startIndividual">
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
                <path d="M8 42c0-8.837 7.163-16 16-16s16 7.163 16 16"
                      stroke="#06195e"
                      stroke-width="2.5"
                      stroke-linecap="round"
                      fill="none" />
              </svg>
            </div>
            <h3 class="modal-title">Select Profile</h3>
            <p class="modal-subtitle">Select the profile of the applicant to continue</p>

            <div class="profile-list">
              <label v-for="profile in profiles"
                     :key="profile.id"
                     class="profile-option"
                     :class="{ selected: selectedProfile === profile.id }">
                <input v-model="selectedProfile"
                       type="radio"
                       :value="profile.id"
                       class="profile-radio-input" />
                <svg class="profile-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <circle cx="10" cy="7" r="3.5" stroke="#06195e" stroke-width="1.5" fill="none" />
                  <path d="M3 18c0-3.866 3.134-7 7-7s7 3.134 7 7"
                        stroke="#06195e"
                        stroke-width="1.5"
                        stroke-linecap="round"
                        fill="none" />
                </svg>
                <span class="profile-name">{{ profile.name }}</span>
                <span class="profile-radio">
                  <span v-if="selectedProfile === profile.id" class="radio-dot" />
                </span>
              </label>
            </div>

            <button class="btn btn-proceed"
                    :disabled="!selectedProfile"
                    @click="proceedWithProfile">
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
            <div v-for="(tab, index) in tabs"
                 :key="tab"
                 :class="['step', { active: currentStep === index, done: currentStep > index }]">
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

                    <!-- File notice banner -->
                    <div class="file-notice">
                      File size limit is 5MB. Supported formats: jpeg, png, bmp, pdf
                    </div>

                    <p class="section-label">Core requirements<span class="required">*</span></p>

                    <div v-for="req in requirements" :key="req.id" class="req-item">
                      <label class="req-item__label">
                        {{ req.label }}<span class="required">*</span>
                      </label>

                      <div class="upload-zone">
                        <!-- Blue Browse Button with + -->
                        <button class="browse-btn" @click.stop="triggerFileInput(req.id)">
                          + Browse
                        </button>

                        <!-- File name display -->
                        <span class="file-name" :class="{ 'file-name--filled': req.file }">
                          {{ req.file ? req.file.name : 'No file chosen' }}
                        </span>

                        <!-- Remove button -->
                        <button v-if="req.file"
                                class="remove-btn"
                                @click.stop="removeFile(req.id)">
                          &times;
                        </button>
                      </div>

                      <!-- Hidden input -->
                      <input :ref="(el) => setFileInputRef(req.id, el)"
                             type="file"
                             accept=".jpeg,.jpg,.bmp,.pdf"
                             class="hidden-input"
                             @change="onFileChange(req.id, $event)" />

                      <!-- Divider line -->
                      <hr class="divider" />
                    </div>
                  </div>
                </div>


                <!-- Step 5 -->
                <div v-else-if="currentStep === 5">
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Payment</h3>
                    </div>

                    <p class="section-description">
                      Review your application summary and complete payment.
                    </p>

                    <!-- Processing Type -->
                    <div class="payment-section">
                      <label class="section-label">Choose a Processing Type<span class="required">*</span></label>
                      <div class="option">
                        <input type="radio" id="regular" value="regular" v-model="processingType" />
                        <label for="regular">Regular Processing (₱950.00) – 12 Working Days</label>
                      </div>
                      <div class="option">
                        <input type="radio" id="special" value="special" v-model="processingType" />
                        <label for="special">Special Processing (₱1,200.00) – Expedited</label>
                      </div>
                    </div>

                    <!-- Payment Method -->
                    <div class="payment-section">
                      <label class="section-label">Choose a Payment Method<span class="required">*</span></label>
                      <div class="option">
                        <input type="radio" id="epayment" value="epayment" v-model="paymentMethod" />
                        <label for="epayment">ePayment (Maya/GCash)</label>
                      </div>
                      <div class="option">
                        <input type="radio" id="card" value="card" v-model="paymentMethod" />
                        <label for="card">Debit/Credit Card</label>
                      </div>
                      <div class="option">
                        <input type="radio" id="otc" value="otc" v-model="paymentMethod" />
                        <label for="otc">Over-the-counter (+₱50.00 convenience fee)</label>
                      </div>
                    </div>

                    <!-- Delivery Options -->
                    <div class="payment-section">
                      <label class="section-label">Delivery Options<span class="required">*</span></label>
                      <div class="option">
                        <input type="radio" id="site" value="site" v-model="deliveryOption" />
                        <label for="site">Site – DFA Manila (ASEANA)</label>
                      </div>
                      <div class="option">
                        <input type="radio" id="address" value="address" v-model="deliveryOption" />
                        <label for="address">Current Address – T1 U1214 LINEAR CONDOMINIUM, MALUGAY, SAN ANTONIO, MAKATI</label>
                      </div>
                    </div>

                    <!-- Summary -->
                    <div class="summary">
                      <h4>Summary</h4>
                      <table>
                        <thead>
                          <tr>
                            <th>Particulars</th>
                            <th>Quantity</th>
                            <th>Amount</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Passport Fee</td>
                            <td>1</td>
                            <td>₱950.00</td>
                          </tr>
                          <tr class="total-row">
                            <td colspan="2"><strong>Total</strong></td>
                            <td><strong>₱950.00</strong></td>
                          </tr>
                        </tbody>
                      </table>
                    </div>

                    <!-- Proceed Button -->
                    <div class="payment-actions">
                      <button class="agree-btn" @click="showPaymentModal = true">Proceed to Payment</button>
                    </div>

                    <!-- Payment Status Panel -->
                    <div v-if="showPaymentStatus" class="status-panel">
                      <h3 class="status-title">Appointment for MARVIN ALFRED MERCADO PICO</h3>
                      <p><strong>This mode of payment is requesting for:</strong></p>
                      <table class="status-table">
                        <tr><td><strong>Status</strong></td><td>Initiated</td></tr>
                        <tr><td><strong>Amount</strong></td><td>₱950.00</td></tr>
                        <tr><td><strong>Total Amount Due</strong></td><td>₱950.00</td></tr>
                      </table>

                      <h4>General Guidelines:</h4>
                      <ul>
                        <li>Pay in CASH at Bayad Center Branches and Authorized Partners...</li>
                        <li>Confirmation Payments are processed once paid.</li>
                        <li>We will send a confirmation email to you once processed.</li>
                        <li>Pay the exact amount. Any excess payment will be forfeited.</li>
                        <li>Amount is inclusive of convenience fee.</li>
                        <li>Make sure to get a reference number before paying.</li>
                        <li>A reference number can only be used once...</li>
                      </ul>

                      <div class="status-actions">
                        <button class="cancel-btn" @click="showPaymentStatus = false">Back</button>
                        <button class="agree-btn" @click="payNow">Pay Now</button>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Payment Confirmation Modal -->
                <div v-if="showPaymentModal" class="modal-overlay">
                  <div class="modal-box">
                    <h3 class="modal-title">Payment Confirmation</h3>
                    <div class="modal-body">
                      <p><strong>Please be reminded of the following:</strong></p>
                      <ul>
                        <li>Your chosen mode of payment will charge a convenience fee in addition to the passport fee.</li>
                        <li>Fees must be settled within 24 hours upon receipt of the Reference Number.</li>
                        <li>Your bank may not be available on weekends and holidays.</li>
                        <li>Amount indicated is exclusively for the payment of the Passport Processing Fee. Processing fee is not refundable and not transferable.</li>
                        <li>Applicants are advised to appear on their scheduled appointment with complete requirements or risk forfeiting payment.</li>
                        <li>Applicants availing of courier service shall submit their current passport for cancellation during their appointment.</li>
                        <li>For TOPS applicants: Passports for pick-up shall be claimed by you or your authorized representative at the designated Supervising Consular Office (SCO) of TOPS. Courier delivery is not available at TOPS sites.</li>
                      </ul>

                      <p><strong>Basahin ang mga sumusunod na paalala:</strong></p>
                      <ul>
                        <li>Ang pinili ninyong mode of payment ay may karagdagang convenience fee, maliban sa passport fee.</li>
                        <li>Ang mga fees ay kailangang mabayaran sa loob ng 24 oras matapos matanggap ang Reference Number. Ang mga bangko o bayad center ay maaaring hindi bukas ng weekend o holidays.</li>
                        <li>Ang amount na nakasaad ay para lamang sa pagbabayad ng Passport Processing Fee.</li>
                        <li>Ang processing fee ay hindi refundable at hindi maaaring i-transfer sa ibang aplikante.</li>
                        <li>Kailangan po ninyong magpunta sa aming tanggapan sa araw ng inyong appointment, dala ang kumpletong passport requirements. Kung hindi makakapunta sa araw ng appointment, ang inyong bayad ay mawawalang bisa.</li>
                        <li>Para sa mga nag-avail ng courier service, kailangang ipresenta ng mga aplikante ang lumang passport para makansela ito ng prosesor.</li>
                        <li>Para sa mga aplikante sa TOPS: Ang mga passports na for pick-up ay kailangang kunin, ninyo o ng inyong representative sa designated na Supervising Consular Office (SCO) ng TOPS. Hindi maaaring mag-request sa TOPS na ipa-deliver ang inyong passport sa araw ng inyong appointment, dahil hindi available ang courier service sa mga TOPS Sites.</li>
                      </ul>
                    </div>

                    <div class="modal-actions">
                      <button class="cancel-btn" @click="showPaymentModal = false">Cancel</button>
                      <button class="agree-btn" @click="agreePaymentModal">I Agree</button>
                    </div>
                  </div>
                </div>


                <!-- Payment Success Modal -->
                <div v-if="showPaymentSuccess" class="modal-overlay">
                  <div class="modal-box success-box">
                    <div class="success-icon">✔</div>
                    <h3 class="modal-title">Payment Successful</h3>
                    <div class="modal-body">
                      <p>Your payment has been successful!</p>
                    </div>
                    <div class="modal-actions">
                      <button class="agree-btn" @click="closePaymentSuccess">Close</button>
                    </div>
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
  import { ref, reactive, computed, onMounted, watch } from 'vue'

  const showPaymentModal = ref(false)
  const showPaymentStatus = ref(false)
  const showPaymentSuccess = ref(false)

  const agreePaymentModal = () => {
    showPaymentModal.value = false
    showPaymentStatus.value = true
  }

  const payNow = () => {
    showPaymentStatus.value = false
    showPaymentSuccess.value = true
  }

  const closePaymentSuccess = () => {
    showPaymentSuccess.value = false
  }

  // ── File Upload Config ──────────────────────────────
  const MAX_FILE_SIZE = 5 * 1024 * 1024 // 5MB
  const SUPPORTED_FORMATS = ['image/jpeg', 'image/jpg', 'image/bmp', 'application/pdf']

  // Props
  const props = defineProps({
    requirements: {
      type: Array,
      default: () => []
    }
  })

  // Reactive data
  const fileInputs = ref({})
  const dragStates = ref({})
  const uploadProgress = ref(0)

  // Computed
  const hasMissingFiles = computed(() => {
    return props.requirements.some(req => !req.file)
  })

  // Methods
  const setFileInputRef = (id, el) => {
    if (el) fileInputs.value[id] = el
  }

  const triggerFileInput = (id) => {
    const input = fileInputs.value[id]
    if (input) input.click()
  }

  const validateFile = (file) => {
    if (file.size > MAX_FILE_SIZE) {
      alert(`File size exceeds 5MB limit. Current size: ${formatFileSize(file.size)}`)
      return false
    }
    if (!SUPPORTED_FORMATS.includes(file.type)) {
      alert('Unsupported file format. Please use jpeg, jpg, bmp, or pdf.')
      return false
    }
    return true
  }

  const onFileChange = (id, event) => {
    const file = event.target.files[0]
    if (file && validateFile(file)) {
      emitFileChange(id, file)
      event.target.value = ''
    }
  }

  const onDragOver = (event, id) => {
    event.dataTransfer.dropEffect = 'copy'
    dragStates.value[id] = true
  }

  const onDragEnter = (event, id) => {
    dragStates.value[id] = true
  }

  const onDragLeave = (event, id) => {
    if (!event.currentTarget.contains(event.relatedTarget)) {
      dragStates.value[id] = false
    }
  }

  const onDrop = (event, id) => {
    dragStates.value[id] = false
    const file = event.dataTransfer.files[0]
    if (file && validateFile(file)) {
      const input = fileInputs.value[id]
      if (input) {
        const dataTransfer = new DataTransfer()
        dataTransfer.items.add(file)
        input.files = dataTransfer.files
        input.dispatchEvent(new Event('change', { bubbles: true }))
      }
    }
  }

  const removeFile = (id) => {
    emitFileChange(id, null)
  }

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  // Emits
  const emit = defineEmits(['file-change'])
  const emitFileChange = (id, file) => {
    emit('file-change', { id, file })
  }

  // Reset drag states on cleanup
  onMounted(() => {
    uploadProgress.value = 0
  })

  watch(() => props.requirements, () => {
    Object.keys(dragStates.value).forEach(id => {
      dragStates.value[id] = false
    })
  }, { deep: true })

  // ── Pre-step state ──────────────────────────────────────────────────
  const preStep = ref("terms")
  const consentChecked = ref(false)
  const profiles = ref([{ id: 1, name: "MARVIN ALFRED MERCADO PICO" }])
  const selectedProfile = ref(null)

  // Pre-step handlers
  const continueExisting = () => alert("Continue existing application - implement routing")
  const startIndividual = () => { if (consentChecked.value) preStep.value = "selectProfile" }
  const startGroup = () => { if (consentChecked.value) preStep.value = "selectProfile" }
  const proceedWithProfile = () => { if (selectedProfile.value) preStep.value = "appointmentNotice" }
  const cancelNotice = () => { preStep.value = "terms" }
  const agreeNotice = () => { preStep.value = null }

  // ── Main stepper state ──────────────────────────────────────────────
  const tabs = [
    "Site Location & Schedule",
    "Application Type",
    "Companions & Assistants",
    "Application Form",
    "Documentary Requirements",
    "Payment",
  ]
  const currentStep = ref(0)

  const nextStep = () => { if (currentStep.value < tabs.length - 1) currentStep.value++ }
  const prevStep = () => { if (currentStep.value > 0) currentStep.value-- }
  const submit = () => alert("Application submitted successfully!")

  // ── Documentary Requirements (Step 4) ──────────────────────────────
  const requirements = ref([
    { id: "gov_id", label: "Valid Government Issued ID", file: null },
    { id: "birth_cert", label: "PSA Birth Certificate or Local Civil Registrar Copy", file: null },
  ])

  const fileInputRefs = ref({})
  const showVerification = ref(false)

  const setFileInputRefStep4 = (id, el) => { if (el) fileInputRefs.value[id] = el }
  const triggerFileInputStep4 = (id) => { fileInputRefs.value[id]?.click() }
  const onFileChangeStep4 = (id, event) => {
    const file = event.target.files[0]
    if (!file) return
    const req = requirements.value.find((r) => r.id === id)
    if (req) { req.file = file; showVerification.value = true }
  }
  const removeFileStep4 = (id) => {
    const req = requirements.value.find((r) => r.id === id)
    if (req) {
      req.file = null
      const input = fileInputRefs.value[id]
      if (input) input.value = ""
    }
    showVerification.value = false
  }
</script>


<style scoped>

  /*  DOCUMENTARIES */
  /* General section styling */
  .success-box {
    text-align: center;
  }

  .success-icon {
    font-size: 48px;
    color: #28a745;
    margin-bottom: 12px;
  }

  .section-description {
    color: #718096;
    font-size: 0.9rem;
    margin-bottom: 16px;
  }

  .payment-section {
    margin: 20px 0;
  }

  .option {
    margin: 6px 0;
  }

  .summary {
    margin-top: 20px;
    border-top: 1px solid #ddd;
    padding-top: 12px;
  }

    .summary table {
      width: 100%;
      border-collapse: collapse;
    }

    .summary th, .summary td {
      border: 1px solid #ddd;
      padding: 8px;
    }

    .summary .total-row {
      background-color: #f1f1f1;
    }

  .payment-actions {
    margin-top: 20px;
  }

  .agree-btn {
    background: #28a745;
    color: #fff;
    padding: 8px 14px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }

  .cancel-btn {
    background: #dc3545;
    color: #fff;
    padding: 8px 14px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }

  /* Modal */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0,0,0,0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-box {
    background: #fff;
    padding: 24px;
    border-radius: 8px;
    width: 600px;
    max-height: 80vh;
    overflow-y: auto;
  }

  .modal-title {
    font-size: 1.2rem;
    font-weight: bold;
    margin-bottom: 12px;
  }

  .modal-body ul {
    margin-left: 20px;
    margin-bottom: 16px;
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 16px;
  }


  .pds-section {
    margin: 20px 0;
    padding: 16px;
    background: #fff;
    border-radius: 6px;
    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  }

  .pds-section-header {
    margin-bottom: 12px;
  }

  .pds-section-title {
    font-size: 1.2rem;
    font-weight: 600;
    color: #333;
  }

  /* Notice banner */
  .file-notice {
    background-color: #fff3cd; /* light yellow */
    color: #856404;
    padding: 10px 14px;
    border: 1px solid #ffeeba;
    border-radius: 4px;
    margin-bottom: 16px;
    font-size: 14px;
  }

  /* Labels */
  .section-label {
    font-weight: 500;
    margin-bottom: 8px;
    display: block;
  }

  .req-item {
    margin-bottom: 16px;
  }

  /* Upload zone */
  .upload-zone {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .browse-btn {
    background-color: #007bff; /* Blue */
    color: #fff;
    border: none;
    padding: 6px 14px;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 500;
  }

    .browse-btn:hover {
      background-color: #0056b3;
    }

  .file-name {
    color: #666;
  }

  .file-name--filled {
    font-weight: 600;
    color: #333;
  }

  .remove-btn {
    background: none;
    border: none;
    color: #d9534f;
    font-size: 18px;
    cursor: pointer;
  }

    .remove-btn:hover {
      color: #b52b27;
    }

  .hidden-input {
    display: none;
  }

  /* Divider line */
  .divider {
    border: none;
    border-bottom: 1px solid #ddd;
    margin: 12px 0;
  }

  /* Verification modal/panel */
  .verification-panel {
    border: 1px solid #ccc;
    padding: 16px;
    margin-top: 20px;
    background: #f9f9f9;
    border-radius: 6px;
  }

    .verification-panel h4 {
      margin-bottom: 8px;
      font-weight: 600;
    }

  /* Payment summary table */
  .summary {
    margin-top: 20px;
    border-top: 1px solid #ddd;
    padding-top: 12px;
  }

    .summary table {
      width: 100%;
      border-collapse: collapse;
    }

    .summary th, .summary td {
      border: 1px solid #ddd;
      padding: 8px;
    }

    .summary tr:last-child {
      background-color: #f1f1f1; /* highlight total row */
      font-weight: bold;
    }



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
    transition: border-color 0.15s, background 0.15s;
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
    transition: opacity 0.25s ease, transform 0.25s ease;
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
    /* ── Documentary Requirements ───────────────────────────────────── */
    .file-notice {
      background: #fffbeb;
      border: 1px solid #f6ad55;
      border-radius: 8px;
      padding: 8px 14px;
      font-size: 0.8rem;
      color: #92400e;
      margin-bottom: 20px;
      text-align: center;
    }

    .section-label {
      font-size: 0.87rem;
      font-weight: 600;
      color: #1a202c;
      margin-bottom: 14px;
    }

    .required {
      color: #e24b4a;
    }

    .req-item {
      margin-bottom: 16px;
    }

    .req-item__label {
      display: block;
      font-size: 0.82rem;
      color: #718096;
      margin-bottom: 6px;
    }

    .upload-zone {
      border: 1.5px dashed #d1d9e6;
      border-radius: 8px;
      padding: 10px 14px;
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      transition: border-color 0.15s, background 0.15s;
      background: #fff;
    }

      .upload-zone:hover {
        border-color: #06195e;
        background: #f0f4ff;
      }

    .upload-zone--filled {
      border-color: #22c55e;
      border-style: solid;
      background: #f0fdf4;
    }

    .browse-btn {
      background: #06195e;
      color: #fff;
      border: none;
      border-radius: 6px;
      padding: 6px 14px;
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
      white-space: nowrap;
      font-family: inherit;
      transition: background 0.15s;
    }

      .browse-btn:hover {
        background: #04134a;
      }

    .file-name {
      font-size: 0.8rem;
      color: #a0aec0;
      flex: 1;
    }

    .file-name--filled {
      color: #16a34a;
      font-weight: 600;
    }

    .remove-btn {
      font-size: 18px;
      line-height: 1;
      color: #a0aec0;
      cursor: pointer;
      padding: 0 2px;
      background: none;
      border: none;
      font-family: inherit;
      transition: color 0.15s;
    }

      .remove-btn:hover {
        color: #e24b4a;
      }

    .hidden-input {
      display: none;
    }
  }
</style>
