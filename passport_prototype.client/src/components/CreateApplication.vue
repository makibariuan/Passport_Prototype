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
                <a href="#" class="notice-link">here</a> for the requirements for passport release.x`
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



                <!-- ══════════════════════════════════════════
       STEP 4 — Documentary Requirements
  ══════════════════════════════════════════ -->
                <div v-else-if="currentStep === 4">
                  <div class="pds-section">

                    <!-- Header -->
                    <p class="appt-title">
                      Appointment for <span class="hname">MARVIN ALFRED MERCADO PICO</span>
                    </p>

                    <!-- File notice banner -->
                    <div class="file-notice">
                      File size limit is 5MB. Supported formats: jpeg, png, bmp, pdf
                    </div>

                    <p class="section-label">Core requirements<span class="required">*</span></p>

                    <div v-for="req in requirements" :key="req.id" class="req-item">
                      <label class="req-item__label">
                        {{ req.label }}<span class="required">*</span>
                      </label>

                      <!-- Browse button -->
                      <div class="upload-zone">
                        <button class="browse-btn" @click.stop="triggerFileInput(req.id)">
                          + Browse
                        </button>
                      </div>

                      <!-- File display row (shown after file attached) -->
                      <div v-if="req.file" class="file-display">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#888" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink:0">
                          <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48" />
                        </svg>
                        <span class="file-name">{{ req.file.name }}</span>
                        <button class="remove-btn" @click.stop="removeFile(req.id)">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                            <line x1="18" y1="6" x2="6" y2="18" />
                            <line x1="6" y1="6" x2="18" y2="18" />
                          </svg>
                        </button>
                      </div>

                      <!-- Hidden file input -->
                      <input :ref="(el) => setFileInputRef(req.id, el)"
                             type="file"
                             accept=".jpeg,.jpg,.png,.bmp,.pdf"
                             class="hidden-input"
                             @change="onFileChange(req.id, $event)" />

                      <hr class="divider" />
                    </div>

                  </div>

                  <!-- ── Conflict Detection Modal ── -->
                  <teleport to="body">
                    <div v-if="showConflictModal" class="modal-overlay" @click.self="closeModal">
                      <div class="modal-box">

                        <!-- Warning banner -->
                        <div class="modal-banner">
                          System found possible conflicts. Please confirm if the attachment is valid.
                        </div>

                        <div class="modal-body">
                          <!-- Conflict comparison table -->
                          <table class="conflict-table">
                            <thead>
                              <tr>
                                <th></th>
                                <th>Detected</th>
                                <th>Inserted</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="row in conflictRows" :key="row.field">
                                <td class="field-name">{{ row.field }}</td>
                                <td>{{ row.detected }}</td>
                                <td>{{ row.inserted }}</td>
                              </tr>
                            </tbody>
                          </table>

                          <!-- ✅ Actual uploaded image preview — not a generated card -->
                          <div class="id-preview-wrap">
                            <img v-if="pendingFilePreviewUrl"
                                 :src="pendingFilePreviewUrl"
                                 class="id-preview-img"
                                 alt="Uploaded ID" />
                            <div v-else class="id-preview-placeholder">
                              Preview not available
                            </div>
                          </div>
                        </div>

                        <!-- Action buttons -->
                        <div class="modal-actions">
                          <button class="btn-confirm" @click="confirmAttachment">Confirm Attachment</button>
                          <button class="btn-reupload" @click="uploadAgain">Upload again</button>
                        </div>

                      </div>
                    </div>
                  </teleport>
                </div>



                <!-- ══════════════════════════════════════════
       STEP 5 — Payment
  ══════════════════════════════════════════ -->
                <div v-else-if="currentStep === 5">
                  <div class="pds-section">

                    <!-- Header -->
                    <p class="appt-title">
                      Appointment for <span class="hname">{{ selectedProfile?.name }}</span>
                    </p>

                    <!-- ── Processing Type ── -->
                    <div class="payment-group">
                      <p class="group-label">Choose a Processing Type</p>

                      <label class="radio-card" :class="{ selected: processingType === 'regular' }">
                        <input type="radio" value="regular" v-model="processingType" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Regular Processing (₱ 950.00)</p>
                          <p class="radio-card__sub">12 Working Days for All Consular Offices</p>
                        </div>
                      </label>

                      <label class="radio-card" :class="{ selected: processingType === 'special' }">
                        <input type="radio" value="special" v-model="processingType" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Special Processing (₱ 1,200.00)</p>
                          <p class="radio-card__sub">a. TOPS Processing, or</p>
                          <p class="radio-card__sub">b. Expedite Processing (Applicable to DFA ASEANA And Consular Offices Only)</p>
                        </div>
                      </label>
                    </div>

                    <!-- ── Payment Method ── -->
                    <div class="payment-group">
                      <p class="group-label">Choose a Payment Method</p>

                      <label class="radio-card" :class="{ selected: paymentMethod === 'epayment' }">
                        <input type="radio" value="epayment" v-model="paymentMethod" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">EPayment</p>
                          <p class="radio-card__sub">Pay via Maya/G Cash</p>
                        </div>
                      </label>

                      <label class="radio-card" :class="{ selected: paymentMethod === 'card' }">
                        <input type="radio" value="card" v-model="paymentMethod" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Debit/Credit Card</p>
                          <p class="radio-card__sub">Pay via Debit or Credit Card</p>
                        </div>
                      </label>

                      <label class="radio-card" :class="{ selected: paymentMethod === 'otc' }">
                        <input type="radio" value="otc" v-model="paymentMethod" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Over the counter Payment to be made at authorized Payment Centers (₱ 50.00 Convenience Fee)</p>
                          <p class="radio-card__sub">Pay via Bayad Center accredited merchant</p>
                        </div>
                      </label>
                    </div>

                    <!-- ── Delivery Options ── -->
                    <div class="payment-group">
                      <p class="group-label">Delivery Options</p>

                      <label class="radio-card" :class="{ selected: deliveryOption === 'site' }">
                        <input type="radio" value="site" v-model="deliveryOption" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Site</p>
                          <p class="radio-card__sub">DFA Manila (Aseana), ASEANA Business Park, Bradco Avenue corner Macapagal Boulevard, Parañaque City</p>
                        </div>
                      </label>

                      <label class="radio-card" :class="{ selected: deliveryOption === 'address' }">
                        <input type="radio" value="address" v-model="deliveryOption" />
                        <div class="radio-card__content">
                          <p class="radio-card__title">Current address</p>
                          <p class="radio-card__sub">T1 U2124 LINEAR CONDOMINIUM, MALUGAY, SAN ANTONIO, MAKATI</p>
                        </div>
                      </label>
                    </div>

                    <!-- ── Summary (shown only when all selected) ── -->
                    <div v-if="processingType && paymentMethod && deliveryOption" class="summary-section">
                      <p class="group-label">Summary</p>
                      <table class="summary-table">
                        <thead>
                          <tr>
                            <th>Particulars</th>
                            <th>Quantity</th>
                            <th class="text-right">Amount</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Passport Fee</td>
                            <td>1</td>
                            <td class="text-right">₱ {{ processingType === 'special' ? '1,200.00' : '950.00' }}</td>
                          </tr>
                          <tr v-if="paymentMethod === 'otc'">
                            <td>Convenience Fee</td>
                            <td>1</td>
                            <td class="text-right">₱ 50.00</td>
                          </tr>
                          <tr class="total-row">
                            <td><strong>TOTAL</strong></td>
                            <td></td>
                            <td class="text-right"><strong>₱ {{ totalAmount }}</strong></td>
                          </tr>
                        </tbody>
                      </table>

                      <!-- Proceed button -->
                      <div class="pay-actions">
                        <button class="btn-proceed" @click="showPaymentModal = true">Proceed to Payment</button>
                      </div>
                    </div>

                  </div>
                </div>

                <!-- ══════════════════════════════════════════
       PAYMENT CONFIRMATION MODAL
  ══════════════════════════════════════════ -->
                <teleport to="body">
                  <div v-if="showPaymentModal" class="modal-overlay" @click.self="showPaymentModal = false">
                    <div class="modal-box payment-modal">
                      <h3 class="modal-title">Payment Confirmation</h3>
                      <div class="modal-scroll-body">
                        <p class="modal-remind"><strong>Please be reminded of the following:</strong></p>
                        <ul class="modal-list">
                          <li>Your chosen mode of payment will charge a convenience fee in addition to the passport fee.</li>
                          <li>Fees must be settled within <strong>24 hours</strong> upon receipt of the Reference Number.</li>
                          <li>Your bank may not be available on weekends and holidays.</li>
                          <li>Amount indicated is exclusively for the payment of the Passport Processing Fee. Processing fee is not refundable and not transferable.</li>
                          <li>Applicants are advised to appear on their scheduled appointment with complete requirements or risk forfeiting payment. Applicants availing of courier service shall submit their current passport for cancellation during their appointment.</li>
                          <li><strong>For TOPS applicants:</strong> Passports for pick-up shall be claimed by you or your authorized representative at the designated Supervising Consular Office (SCO) of TOPS. There are no available courier delivery services on the TOPS site. Further, applicants at TOPS Sites will be unable to request the delivery of their passports on the day of their appointment schedule.</li>
                        </ul>

                        <p class="modal-remind"><strong>Basahin ang mga sumusunod na paalala:</strong></p>
                        <ul class="modal-list">
                          <li>Ang pinili ninyong mode of payment ay may karagdagang convenience fee, maliban sa passport fee.</li>
                          <li>Ang mga fees ay kailangang mabayaran sa loob ng 24 oras matapos matanggap ang Reference Number. Ang mga bangko o bayad center ay maaaring hindi bukas ng weekend o holidays.</li>
                          <li>Ang amount na nakasaad ay para lamang sa pagbabayad ng Passport Processing Fee.</li>
                          <li>Ang processing fee ay hindi refundable at hindi maaaring i-transfer sa ibang aplikante.</li>
                          <li>Kailangan po ninyong magpunta sa aming tanggapan sa araw ng inyong appointment, dala ang kumpletong passport requirements. Kung hindi makakapunta sa araw ng appointment, ang inyong bayad ay mawawalang bisa.</li>
                          <li>Para sa mga nag-avail ng courier service, kailangang ipresenta ng mga aplikante ang lumang passport para makansela ito ng prosesor.</li>
                          <li>Para sa mga aplikante sa TOPS: Ang mga passports na for pick-up ay kailangang kunin, ninyo o ng inyong representative sa designated na Supervising Consular Office (SCO) ng TOPS. Hindi maaaring mag-request sa TOPS na ipa-deliver ang inyong passport sa araw ng inyong appointment, dahil hindi available ang courier service sa mga TOPS Sites.</li>
                        </ul>
                      </div>
                      <div class="modal-foot">
                        <button class="btn-cancel" @click="showPaymentModal = false">Cancel</button>
                        <button class="btn-agree" @click="agreePaymentModal">I Agree</button>
                      </div>
                    </div>
                  </div>
                </teleport>

                <!-- ══════════════════════════════════════════
       PAYMENT SUCCESS MODAL
  ══════════════════════════════════════════ -->
                <teleport to="body">
                  <div v-if="showPaymentSuccess" class="modal-overlay" @click.self="closePaymentSuccess">
                    <div class="modal-box modal-small">
                      <!-- Green check circle -->
                      <div class="success-icon-wrap">
                        <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
                          <circle cx="24" cy="24" r="23" stroke="#34a853" stroke-width="2" />
                          <polyline points="14,24 21,31 34,17" stroke="#34a853" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                      </div>
                      <h3 class="modal-title">Payment successful</h3>
                      <p class="success-sub">Your payment has been successful!</p>
                      <div class="modal-foot modal-foot--center">
                        <button class="btn-agree" @click="closePaymentSuccess">Close</button>
                      </div>
                    </div>
                  </div>
                </teleport>

                <!-- ══════════════════════════════════════════
       DFA ePAYMENT RECEIPT MODAL
  ══════════════════════════════════════════ -->
                <teleport to="body">
                  <div v-if="showEReceipt" class="modal-overlay" @click.self="showEReceipt = false">
                    <div class="modal-box modal-small">
                      <!-- DFA header -->
                      <div class="dfa-header">
                        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/5/5c/Seal_of_the_Department_of_Foreign_Affairs_of_the_Philippines.svg/240px-Seal_of_the_Department_of_Foreign_Affairs_of_the_Philippines.svg.png"
                             class="dfa-seal" alt="DFA Seal" />
                        <div>
                          <p class="dfa-dept">Department of Foreign Affairs</p>
                          <p class="dfa-epay">ePayment Services</p>
                        </div>
                      </div>

                      <p class="epay-intro">This mode of payment is requesting for...</p>

                      <table class="epay-table">
                        <tr>
                          <td class="ep-label">Status</td>
                          <td class="ep-value ep-bold">PAID</td>
                        </tr>
                        <tr>
                          <td class="ep-label">Scheduled Date</td>
                          <td class="ep-value">9 January 2025, 09:00</td>
                        </tr>
                        <tr>
                          <td class="ep-label">Amount</td>
                          <td class="ep-value">PHP 950.00</td>
                        </tr>
                        <tr>
                          <td class="ep-label">Total Amount Due</td>
                          <td class="ep-value ep-green">PHP 950.00</td>
                        </tr>
                      </table>

                      <p class="not-receipt">This is not your e-Receipt.</p>

                      <div class="modal-foot modal-foot--center">
                        <button class="btn-agree" @click="showEReceipt = false">Close</button>
                      </div>
                    </div>
                  </div>
                </teleport>



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






<!-- ════════════════════════════════════════
     SCRIPT
═══════════════════════════════════════════ -->
<script setup>
  import LeftMenu from "@/components/LeftMenu.vue"
  import { ref, computed } from 'vue'

  // Add these to your existing script — merge with what you already have

  // ── Payment step state ──────────────────────────────────────────────
  const processingType = ref(null)   // 'regular' | 'special'
  const paymentMethod = ref(null)   // 'epayment' | 'card' | 'otc'
  const deliveryOption = ref(null)   // 'site' | 'address'

  // ── Modal visibility ────────────────────────────────────────────────
  const showPaymentModal = ref(false)
  const showPaymentStatus = ref(false)
  const showPaymentSuccess = ref(false)
  const showEReceipt = ref(false)   // Image 6 — DFA ePayment receipt

  // ── Computed total ──────────────────────────────────────────────────
  const totalAmount = computed(() => {
    let base = processingType.value === 'special' ? 1200 : 950
    if (paymentMethod.value === 'otc') base += 50
    return base.toLocaleString('en-PH', { minimumFractionDigits: 2 })
  })

  // ── Payment flow handlers ───────────────────────────────────────────
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
    showEReceipt.value = true   // show DFA receipt after success
  }

  // ── Pre-step state ──────────────────────────────────────────────────
  const preStep = ref("terms")
  const consentChecked = ref(false)
  const profiles = ref([{ id: 1, name: "MARVIN ALFRED MERCADO PICO" }])
  const selectedProfile = ref(null)

  const continueExisting = () => alert("Continue existing application")
  const startIndividual = () => { if (consentChecked.value) preStep.value = "selectProfile" }
  const startGroup = () => { if (consentChecked.value) preStep.value = "selectProfile" }
  const proceedWithProfile = () => { if (selectedProfile.value) preStep.value = "appointmentNotice" }
  const cancelNotice = () => { preStep.value = "terms" }
  const agreeNotice = () => { preStep.value = null }

  // ── Main stepper ────────────────────────────────────────────────────
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

  // ── Step 4: Documentary Requirements ───────────────────────────────
  const MAX_FILE_SIZE = 5 * 1024 * 1024
  const SUPPORTED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/bmp', 'application/pdf']
  const CONFLICT_IDS = ['gov-id'] // only gov-id triggers conflict modal

  const requirements = ref([
    { id: 'gov-id', label: 'Valid Government Issued ID', file: null },
    { id: 'birth-cert', label: 'PSA Birth Certificate or Local Civil Registrar Copy', file: null },
  ])

  const fileInputs = ref({})
  const showConflictModal = ref(false)
  const pendingFile = ref(null)             // { id, file }
  const pendingFilePreviewUrl = ref(null)             // ← base64 data URL of the uploaded image

  // ── OCR / backend detected data ─────────────────────────────────────
  // Replace these values with your actual API/OCR response
  const detectedData = ref({
    crn: '0028-1215160-9',
    surname: 'SANTOS',
    givenName: 'JOSE',
    middleName: 'CRUZ',
    sex: 'M',
    dob: '1960/01/28',
    addressLine1: '28 PAYAPA ST BAGONG DIWA',
    addressLine2: 'STO CRISTOBAL CALOOCAN CITY',
    addressLine3: 'METRO MANILA PHILIPPINES 1800',
  })

  // ── Conflict table rows ─────────────────────────────────────────────
  const conflictRows = computed(() => [
    {
      field: 'Full Name',
      detected: `${detectedData.value.givenName} ${detectedData.value.middleName} ${detectedData.value.surname}`,
      inserted: selectedProfile.value?.name ?? ''
    },
    {
      field: 'Address',
      detected: [detectedData.value.addressLine1, detectedData.value.addressLine2, detectedData.value.addressLine3].join(' '),
      inserted: 'T1 U2124 LINEAR CONDOMINIUM, MALUGAY, SAN ANTONIO, MAKATI'
    },
    { field: 'Date Of Birth', detected: '1960-01-28', inserted: '1988-06-26' },
    { field: 'Gender', detected: detectedData.value.sex, inserted: 'M' },
    { field: 'Place Of Birth', detected: '', inserted: '' },
    { field: 'Occupation', detected: '', inserted: '' },
    { field: 'Nationality', detected: '', inserted: 'PHILIPPINES, FILIPINO' },
  ])

  const hasMissingFiles = computed(() => requirements.value.some(r => !r.file))

  // ── File input refs ─────────────────────────────────────────────────
  const setFileInputRef = (id, el) => { if (el) fileInputs.value[id] = el }
  const triggerFileInput = (id) => { fileInputs.value[id]?.click() }

  // ── Validation ──────────────────────────────────────────────────────
  const validateFile = (file) => {
    if (file.size > MAX_FILE_SIZE) {
      alert(`File exceeds 5MB. Current size: ${formatFileSize(file.size)}`)
      return false
    }
    if (!SUPPORTED_TYPES.includes(file.type)) {
      alert('Unsupported format. Use jpeg, jpg, png, bmp, or pdf.')
      return false
    }
    return true
  }

  // ── File change handler ─────────────────────────────────────────────
  const onFileChange = (id, event) => {
    const file = event.target.files[0]
    if (!file) return
    if (!validateFile(file)) { event.target.value = ''; return }

    if (CONFLICT_IDS.includes(id)) {
      pendingFile.value = { id, file }

      // ✅ Read the file as a data URL so we can show the actual image in the modal
      const reader = new FileReader()
      reader.onload = (e) => {
        pendingFilePreviewUrl.value = e.target.result
        showConflictModal.value = true
      }
      reader.readAsDataURL(file)
    } else {
      attachFile(id, file)
    }

    event.target.value = ''
  }

  const removeFile = (id) => {
    const req = requirements.value.find(r => r.id === id)
    if (req) req.file = null
  }

  // ── Modal actions ───────────────────────────────────────────────────
  const confirmAttachment = () => {
    if (pendingFile.value) {
      attachFile(pendingFile.value.id, pendingFile.value.file)
      pendingFile.value = null
    }
    pendingFilePreviewUrl.value = null
    showConflictModal.value = false
  }

  const uploadAgain = () => {
    pendingFile.value = null
    pendingFilePreviewUrl.value = null
    showConflictModal.value = false
    setTimeout(() => fileInputs.value['gov-id']?.click(), 100)
  }

  const closeModal = () => {
    pendingFile.value = null
    pendingFilePreviewUrl.value = null
    showConflictModal.value = false
  }

  // ── Helpers ─────────────────────────────────────────────────────────
  const attachFile = (id, file) => {
    const req = requirements.value.find(r => r.id === id)
    if (req) req.file = file
  }

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }
</script>




<style scoped>

  /* DOCUMENTARIES DESIGN */
  /* ── Documentary Requirements ───────────────────────────────────── */
  /* ── Page / Section ─────────────────────────────── */
  .pds-section {
    background: #fff;
    border: 1px solid #dde3ec;
    border-radius: 4px;
    padding: 20px 28px 22px;
  }

  /* ── Appointment title ────────────────────────────────────────── */
  .appt-title {
    font-size: 14px;
    font-weight: 700;
    color: #222;
    margin-bottom: 14px;
  }

  .hname {
    color: #1a6fc4;
    font-weight: 700;
    text-decoration: none;
  }

  /* ── File notice banner ───────────────────────────────────────── */
  .file-notice {
    background: #fdf8ec;
    border: 1px solid #f0d99a;
    color: #7a5c00;
    font-size: 11.5px;
    text-align: center;
    padding: 7px 12px;
    border-radius: 3px;
    margin-bottom: 18px;
  }

  /* ── Labels ───────────────────────────────────────────────────── */
  .section-label {
    font-size: 12.5px;
    font-weight: 600;
    color: #333;
    margin-bottom: 14px;
  }

  .required {
    color: #d32f2f;
    margin-left: 1px;
  }

  .req-item {
    margin-bottom: 4px;
  }

  .req-item__label {
    display: block;
    font-size: 12px;
    color: #555;
    margin-bottom: 8px;
  }

  /* ── Browse button ────────────────────────────────────────────── */
  .upload-zone {
    margin-bottom: 2px;
  }

  .browse-btn {
    background: #1a6fc4;
    color: #fff;
    border: none;
    border-radius: 3px;
    font-size: 12px;
    padding: 6px 16px;
    cursor: pointer;
    font-weight: 500;
    display: inline-flex;
    align-items: center;
    gap: 4px;
    transition: background-color 0.2s ease;
  }

    .browse-btn:hover {
      background: #155fa0;
    }

  /* ── File display row ─────────────────────────────────────────── */
  .file-display {
    display: flex;
    align-items: center;
    gap: 8px;
    border: 1px solid #dce3ec;
    border-radius: 3px;
    padding: 5px 10px;
    margin-top: 7px;
    background: #f8fafc;
    max-width: 420px;
  }

  .file-name {
    flex: 1;
    font-size: 12px;
    color: #444;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .remove-btn {
    background: none;
    border: none;
    color: #d32f2f;
    cursor: pointer;
    padding: 0 2px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    transition: color 0.2s ease;
  }

    .remove-btn:hover {
      color: #b71c1c;
    }

  .hidden-input {
    display: none;
  }

  /* ── Divider ──────────────────────────────────────────────────── */
  .divider {
    border: none;
    border-top: 1px solid #e8edf3;
    margin: 14px 0 10px;
  }

  /* ── Modal overlay ────────────────────────────────────────────── */
  .modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 1000;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  /* ── Modal box ────────────────────────────────────────────────── */
  .modal-box {
    background: #fff;
    border-radius: 6px;
    width: 520px;
    max-width: 96vw;
    max-height: 92vh;
    overflow-y: auto;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.22);
  }

  /* ── Modal warning banner ─────────────────────────────────────── */
  .modal-banner {
    background: #fdf8ec;
    border-bottom: 1px solid #f0d99a;
    color: #7a5c00;
    font-size: 12.5px;
    text-align: center;
    padding: 11px 18px;
    border-radius: 6px 6px 0 0;
  }

  /* ── Modal body ───────────────────────────────────────────────── */
  .modal-body {
    padding: 16px 20px 8px;
  }

  /* ── Conflict table ───────────────────────────────────────────── */
  .conflict-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12.5px;
    margin-bottom: 6px;
  }

    .conflict-table thead th {
      text-align: left;
      font-weight: 700;
      color: #333;
      padding: 5px 12px 9px;
      border-bottom: 1px solid #e0e0e0;
    }

      .conflict-table thead th:first-child {
        width: 24%;
      }

    .conflict-table tbody td {
      padding: 7px 12px;
      vertical-align: top;
      border-bottom: 0.5px solid #f0f0f0;
      color: #555;
      font-size: 12px;
      line-height: 1.45;
    }

      .conflict-table tbody td.field-name {
        font-weight: 600;
        color: #333;
        white-space: nowrap;
      }

  /* ── Actual uploaded ID image preview ─────────────────────────── */
  .id-preview-wrap {
    width: 80%;
    margin: 14px auto 16px;
    border-radius: 6px;
    overflow: hidden;
    border: 1px solid #ccc;
    background: #f0f0f0;
    min-height: 80px;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .id-preview-img {
    width: 100%;
    display: block;
    border-radius: 5px;
  }

  .id-preview-placeholder {
    font-size: 12px;
    color: #999;
    padding: 20px;
  }

  /* ── Modal action buttons ─────────────────────────────────────── */
  .modal-actions {
    display: flex;
    gap: 12px;
    justify-content: center;
    padding: 6px 20px 20px;
  }

  .btn-confirm {
    background: #388e3c;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    font-weight: 600;
    padding: 9px 24px;
    cursor: pointer;
    transition: background-color 0.2s ease;
  }

    .btn-confirm:hover {
      background: #2e7d32;
    }

  .btn-reupload {
    background: #d32f2f;
    color: #fff;
    border: none;
    border-radius: 3px;
    font-size: 13px;
    font-weight: 600;
    padding: 9px 24px;
    cursor: pointer;
    transition: background-color 0.2s ease;
  }

    .btn-reupload:hover {
      background: #b71c1c;
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

    /* step  5  */
    /* ── Section wrapper ──────────────────────────────────────────── */
    .pds-section {
      background: #fff;
      border: 1px solid #dde3ec;
      border-radius: 4px;
      padding: 20px 28px 28px;
    }

    /* ── Appointment title ────────────────────────────────────────── */
    .appt-title {
      font-size: 14px;
      font-weight: 700;
      color: #222;
      margin-bottom: 20px;
    }

    .name-highlight {
      color: #1a6fc4;
      font-weight: 700;
    }

    /* ── Group label ──────────────────────────────────────────────── */
    .group-label {
      font-size: 12.5px;
      font-weight: 600;
      color: #333;
      margin-bottom: 8px;
    }

    /* ── Payment group spacing ────────────────────────────────────── */
    .payment-group {
      margin-bottom: 22px;
    }

    /* ── Radio card ───────────────────────────────────────────────── */
    .radio-card {
      display: flex;
      align-items: flex-start;
      gap: 10px;
      border: 1px solid #dde3ec;
      border-radius: 3px;
      padding: 10px 14px;
      margin-bottom: 6px;
      cursor: pointer;
      background: #fff;
      transition: background 0.15s;
    }

      .radio-card:hover {
        background: #f5f8fd;
      }

      .radio-card.selected {
        background: #eef4fd;
        border-color: #aac8f0;
      }

      .radio-card input[type="radio"] {
        margin-top: 3px;
        accent-color: #1a6fc4;
        flex-shrink: 0;
      }

    .radio-card__content {
      flex: 1;
    }

    .radio-card__title {
      font-size: 12.5px;
      font-weight: 600;
      color: #222;
      margin-bottom: 2px;
    }

    .radio-card__sub {
      font-size: 11.5px;
      color: #666;
      margin: 0;
      line-height: 1.5;
    }

    /* ── Summary table ────────────────────────────────────────────── */
    .summary-section {
      margin-top: 6px;
    }

    .summary-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 12.5px;
      margin-bottom: 16px;
    }

      .summary-table thead th {
        text-align: left;
        font-weight: 600;
        color: #555;
        font-size: 12px;
        padding: 8px 12px;
        border: 1px solid #dde3ec;
        background: #f7f9fc;
      }

      .summary-table tbody td {
        padding: 8px 12px;
        border: 1px solid #dde3ec;
        color: #333;
        font-size: 12.5px;
      }

      .summary-table .text-right {
        text-align: right;
      }

      .summary-table .total-row td {
        background: #f7f9fc;
        font-size: 12.5px;
      }

    /* ── Proceed button ───────────────────────────────────────────── */
    .pay-actions {
      display: flex;
      justify-content: flex-end;
      margin-top: 8px;
    }

    .btn-proceed {
      background: #1a6fc4;
      color: #fff;
      border: none;
      border-radius: 3px;
      font-size: 12.5px;
      font-weight: 600;
      padding: 8px 22px;
      cursor: pointer;
    }

      .btn-proceed:hover {
        background: #155fa0;
      }

    /* ── Status panel ─────────────────────────────────────────────── */
    .status-panel {
      margin-top: 24px;
      border-top: 1px solid #e8edf3;
      padding-top: 22px;
    }

    .status-intro {
      font-size: 12.5px;
      color: #555;
      margin-bottom: 14px;
    }

    .status-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }

      .status-table td {
        padding: 10px 0;
        border-bottom: 1px solid #eee;
        font-size: 13px;
      }

    .st-label {
      color: #555;
      font-weight: 500;
      width: 200px;
    }

    .st-value {
      color: #222;
      text-align: right;
    }

    .st-initiated {
      color: #1a6fc4;
      font-weight: 600;
    }

    .st-green {
      color: #2e7d32;
      font-weight: 600;
    }

    .guidelines-title {
      font-size: 12.5px;
      font-weight: 600;
      color: #333;
      margin-bottom: 8px;
    }

    .guidelines-list {
      font-size: 12px;
      color: #444;
      padding-left: 20px;
      line-height: 1.7;
      margin-bottom: 14px;
    }

      .guidelines-list li {
        margin-bottom: 4px;
      }

    .guidelines-link {
      font-size: 12px;
      color: #555;
      margin-bottom: 16px;
    }

    .link-blue {
      color: #1a6fc4;
      text-decoration: underline;
      cursor: pointer;
    }

    .status-nav {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      margin-top: 16px;
    }

    .btn-back-pay {
      background: #fff;
      color: #555;
      border: 1px solid #bbb;
      border-radius: 3px;
      font-size: 12px;
      padding: 6px 20px;
      cursor: pointer;
    }

      .btn-back-pay:hover {
        background: #f0f0f0;
      }

    .btn-pay-now {
      background: #1a6fc4;
      color: #fff;
      border: none;
      border-radius: 3px;
      font-size: 12px;
      font-weight: 600;
      padding: 6px 20px;
      cursor: pointer;
    }

      .btn-pay-now:hover {
        background: #155fa0;
      }

    /* ── Modal overlay ────────────────────────────────────────────── */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.45);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    /* ── Modal box ────────────────────────────────────────────────── */
    .modal-box {
      background: #fff;
      border-radius: 6px;
      width: 580px;
      max-width: 96vw;
      max-height: 88vh;
      display: flex;
      flex-direction: column;
      box-shadow: 0 8px 32px rgba(0,0,0,0.18);
    }

    .modal-small {
      width: 340px;
    }

    /* ── Modal title ──────────────────────────────────────────────── */
    .modal-title {
      font-size: 15px;
      font-weight: 700;
      color: #222;
      text-align: center;
      padding: 18px 22px 0;
      margin: 0;
    }

    /* ── Modal scrollable body ────────────────────────────────────── */
    .modal-scroll-body {
      overflow-y: auto;
      padding: 14px 24px 10px;
      flex: 1;
    }

    .modal-remind {
      font-size: 12.5px;
      color: #333;
      margin-bottom: 8px;
      margin-top: 12px;
    }

    .modal-list {
      font-size: 12px;
      color: #444;
      padding-left: 20px;
      line-height: 1.7;
      margin-bottom: 6px;
    }

      .modal-list li {
        margin-bottom: 4px;
      }

    /* ── Modal footer ─────────────────────────────────────────────── */
    .modal-foot {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      padding: 12px 22px 18px;
      border-top: 1px solid #eee;
    }

    .modal-foot--center {
      justify-content: center;
      border-top: none;
      padding-top: 6px;
    }

    .btn-cancel {
      background: #fff;
      color: #555;
      border: 1px solid #bbb;
      border-radius: 3px;
      font-size: 12.5px;
      padding: 7px 22px;
      cursor: pointer;
    }

      .btn-cancel:hover {
        background: #f0f0f0;
      }

    .btn-agree {
      background: #1a6fc4;
      color: #fff;
      border: none;
      border-radius: 3px;
      font-size: 12.5px;
      font-weight: 600;
      padding: 7px 22px;
      cursor: pointer;
    }

      .btn-agree:hover {
        background: #155fa0;
      }

    /* ── Payment Success Modal ────────────────────────────────────── */
    .success-icon-wrap {
      display: flex;
      justify-content: center;
      padding: 22px 0 8px;
    }

    .success-sub {
      font-size: 13px;
      color: #555;
      text-align: center;
      margin: 0 0 14px;
    }

    /* ── DFA ePayment Receipt Modal ───────────────────────────────── */
    .dfa-header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 18px 22px 12px;
      border-bottom: 1px solid #eee;
    }

    .dfa-seal {
      width: 44px;
      height: 44px;
      flex-shrink: 0;
    }

    .dfa-dept {
      font-size: 12px;
      font-weight: 700;
      color: #222;
      margin: 0;
    }

    .dfa-epay {
      font-size: 12px;
      color: #1a6fc4;
      font-weight: 600;
      margin: 0;
    }

    .epay-intro {
      font-size: 12px;
      color: #666;
      padding: 10px 22px 4px;
      margin: 0;
    }

    .epay-table {
      width: 100%;
      border-collapse: collapse;
      padding: 0 22px;
      font-size: 13px;
    }

      .epay-table td {
        padding: 10px 22px;
        border-bottom: 1px solid #f0f0f0;
      }

    .ep-label {
      color: #555;
      font-size: 12.5px;
    }

    .ep-value {
      color: #222;
      text-align: right;
      font-size: 13px;
    }

    .ep-bold {
      font-weight: 700;
    }

    .ep-green {
      color: #2e7d32;
      font-weight: 700;
    }

    .not-receipt {
      font-size: 12px;
      color: #e0900a;
      text-align: center;
      padding: 12px 22px 4px;
      margin: 0;
    }
  }
</style>
