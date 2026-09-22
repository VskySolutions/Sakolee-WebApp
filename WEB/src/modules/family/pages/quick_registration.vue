<template>
  <q-page padding>
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Families' },
        { label: 'Quick Registration' }
      ]"
      :back-to="{ name: 'dashboard' }"
    />

    <q-form ref="formRef" greedy class="column q-gutter-md" @submit.prevent="submitForm">
      <!-- Step 1 — Family & Studio Information -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 1: Family &amp; Studio Information</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-3">
            <app-select v-model="form.heardAbout" label="How Did You Hear About Us?" :options="HEARD_ABOUT_OPTIONS" />
          </div>
          <div class="col-12 col-md-3">
            <app-text-field v-model="form.referralName" label="Referral Name" placeholder="Friend or student name (optional)" />
          </div>
          <div class="col-12 col-md-3">
            <app-select v-model="form.studioLocation" label="Studio Location" required :options="STUDIO_LOCATION_OPTIONS" :rules="[required]" />
          </div>
          <div class="col-12 col-md-3">
            <app-text-field v-model="form.familyName" label="Family Name" required placeholder="e.g. Miller Family" :rules="[required]" />
          </div>
        </div>
      </q-card>

      <!-- Step 2 — Contact Information -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 2: Contact Information</div>
        <div v-for="contact in contactSlots" :key="contact.key" class="q-mb-sm">
          <div class="step-subtitle">{{ contact.heading }}</div>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-3">
              <app-text-field
                v-model="contact.model.firstName"
                label="First Name"
                placeholder="First Name"
                :required="contact.required"
                :rules="contact.required ? [required] : []"
              />
            </div>
            <div class="col-12 col-md-3">
              <app-text-field
                v-model="contact.model.lastName"
                label="Last Name"
                placeholder="Last Name"
                :required="contact.required"
                :rules="contact.required ? [required] : []"
              />
            </div>
            <div class="col-12 col-md-2">
              <app-select v-model="contact.model.relation" label="Type / Relation" :options="RELATION_OPTIONS" />
            </div>
            <div class="col-12 col-md-2">
              <app-text-field
                v-model="contact.model.email"
                label="Email"
                type="email"
                placeholder="primary@example.com"
                :required="contact.required"
                :rules="contact.required ? [required, emailRule] : [optionalEmail]"
              />
            </div>
            <div class="col-12 col-md-2">
              <app-phone-input v-model="contact.model.phone" v-model:country="contact.model.phoneCountry" label="Cell Phone" />
            </div>
          </div>
        </div>
      </q-card>

      <!-- Step 3 — Address & Emergency Details -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 3: Address &amp; Emergency Details</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <app-text-field v-model="form.streetAddress" label="Street Address" required placeholder="123 Main Street, Apt 4B" :rules="[required]" />
          </div>
          <div class="col-12 col-md-2">
            <app-text-field v-model="form.city" label="City" required placeholder="Chicago" :rules="[required]" />
          </div>
          <div class="col-12 col-md-2">
            <app-text-field v-model="form.state" label="State" required placeholder="IL" :rules="[required]" />
          </div>
          <div class="col-12 col-md-2">
            <app-text-field v-model="form.postalCode" label="ZIP Code" required placeholder="60601" :rules="[required]" />
          </div>
          <div class="col-12 col-md-4">
            <app-text-field v-model="form.emergencyContactName" label="Emergency Contact Person" required placeholder="Full name and relationship" :rules="[required]" />
          </div>
          <div class="col-12 col-md-4">
            <app-phone-input v-model="form.emergencyPhone" v-model:country="form.emergencyPhoneCountry" label="Emergency Phone" required />
          </div>
          <div class="col-12 col-md-4">
            <app-text-field v-model="form.insuranceCarrier" label="Health Insurance Carrier / Policy #" placeholder="e.g. BlueCross BlueShield #9823471" />
          </div>
        </div>
      </q-card>

      <!-- Step 4 — Student Profile -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 4: Student Profile</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-3">
            <app-text-field v-model="form.student.firstName" label="First Name" required placeholder="Student First Name" :rules="[required]" />
          </div>
          <div class="col-12 col-md-3">
            <app-text-field v-model="form.student.lastName" label="Last Name" required placeholder="Student Last Name" :rules="[required]" />
          </div>
          <div class="col-12 col-md-3">
            <app-date-field v-model="form.student.birthDate" label="Birth Date" required :rules="[required]" />
          </div>
          <div class="col-12 col-md-3">
            <app-select v-model="form.student.gender" label="Gender" :options="GENDER_OPTIONS" />
          </div>
          <div class="col-12 col-md-3">
            <app-select v-model="form.student.tshirtSize" label="T-Shirt Size" :options="TSHIRT_SIZE_OPTIONS" />
          </div>
          <div class="col-12 col-md-3">
            <app-text-field v-model="form.student.gradeLevel" label="Grade Level" placeholder="e.g. 3rd Grade" />
          </div>
          <div class="col-12 col-md-6">
            <app-text-field
              v-model="form.student.medicalNotes"
              label="Allergies, Special Needs &amp; Medical Notes"
              type="textarea"
              autogrow
              placeholder="List any peanut allergies, asthma or medical requirements..."
            />
          </div>
        </div>
      </q-card>

      <!-- Step 5 — Class Enrollment -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 5: Class Enrollment</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <app-select v-model="form.className" label="Choose Class" required :options="CLASS_OPTIONS" :rules="[required]" />
          </div>
          <div class="col-12 col-md-3">
            <app-date-field v-model="form.enrollmentDate" label="Enrollment Date" />
          </div>
        </div>
      </q-card>

      <!-- Step 6 — Payment Schedule & Verification -->
      <q-card flat bordered class="q-pa-md">
        <div class="step-title">Step 6: Payment Schedule &amp; Verification</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-4">
            <app-select v-model="form.paymentMethod" label="Payment Method" :options="PAYMENT_METHOD_OPTIONS" :clearable="false" />
          </div>
          <div v-if="form.paymentMethod !== 'invoice'" class="col-12 col-md-8">
            <!-- The prototype types the card number, expiry and CVV straight into the page. Card data
                 is not collected here: the gateway's own hosted fields mount in this slot and hand
                 back a token, so the raw number never reaches this form or our servers. -->
            <app-field-label label="Card Details" />
            <div class="payment-slot">
              <q-icon name="o_lock" size="18px" class="q-mr-sm" />
              <span>Card details are entered in the payment gateway's secure fields, which mount here once the gateway is configured.</span>
            </div>
          </div>
        </div>
      </q-card>

      <div class="row justify-end q-gutter-sm">
        <q-btn flat no-caps label="Cancel" color="grey" :to="{ name: 'dashboard' }" />
        <q-btn unelevated no-caps color="primary" label="Register Family" :loading="saving" @click="submitForm" />
      </div>
    </q-form>
  </q-page>
</template>

<script setup>
import { computed, reactive, ref } from "vue";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppPhoneInput from "components/common/AppPhoneInput.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";

import { useNotify } from "composables/useNotify";
import {
  blankQuickRegistrationForm,
  toQuickRegistrationPayload,
  HEARD_ABOUT_OPTIONS,
  STUDIO_LOCATION_OPTIONS,
  RELATION_OPTIONS,
  GENDER_OPTIONS,
  TSHIRT_SIZE_OPTIONS,
  CLASS_OPTIONS,
  PAYMENT_METHOD_OPTIONS
} from "composables/quickRegistrationForm";

const notify = useNotify();

const formRef = ref(null);
const saving = ref(false);
const form = reactive(blankQuickRegistrationForm());

// The prototype repeats the same five fields for a primary and a second contact; only the first
// is mandatory.
const contactSlots = computed(() => [
  { key: "primary", heading: "Primary Contact", model: form.primaryContact, required: true },
  { key: "secondary", heading: "Second Contact (optional)", model: form.secondaryContact, required: false }
]);

const required = (val) => (val !== null && val !== undefined && String(val).trim() !== "") || "Required";
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const emailRule = (val) => EMAIL_RE.test(String(val || "")) || "Enter a valid email";
const optionalEmail = (val) => !String(val || "").trim() || emailRule(val);

const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    // No intake endpoint exists yet — there is no Family entity or controller in the domain. The
    // payload is built here so the call is a one-line addition once there is:
    //   await familyApi.quickRegister(toQuickRegistrationPayload(form));
    toQuickRegistrationPayload(form);
    notify.warning("Registration can't be saved yet — the Families API isn't available.");
  } finally {
    saving.value = false;
  }
};
</script>

<style scoped>
.step-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--on-surface);
  margin-bottom: 12px;
}

.step-subtitle {
  font-size: 12px;
  font-weight: 600;
  color: var(--outline);
  margin-bottom: 8px;
}

.payment-slot {
  display: flex;
  align-items: center;
  font-size: 12px;
  color: var(--on-surface-variant);
  background: var(--surface-container-low);
  border: 1px dashed var(--outline-variant);
  border-radius: 8px;
  padding: 10px 12px;
}
</style>
