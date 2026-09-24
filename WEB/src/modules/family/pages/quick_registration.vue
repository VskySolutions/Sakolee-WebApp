<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Families', to: '/families' }, { label: 'Quick Registration' }]"
      title="Quick Registration"
      description="Quick registration form."
    />

    <!-- Step progress tracker -->
    <q-card flat bordered class="wizard-tracker-card">
      <div class="wizard-tracker">
        <div class="wizard-tracker__line" />
        <div class="wizard-tracker__line-fill" :style="{ width: fillPercent + '%' }" />
        <button
          v-for="s in STEPS" :key="s.number" type="button"
          class="wizard-step-node"
          :disabled="s.number > currentStep"
          @click="goToStep(s.number)"
        >
          <span
            class="wizard-step-badge"
            :class="{ 'wizard-step-badge--done': s.number < currentStep, 'wizard-step-badge--active': s.number === currentStep }"
          >
            <q-icon v-if="s.number < currentStep" name="o_check" size="16px" />
            <template v-else>{{ s.number }}</template>
          </span>
          <span
            class="wizard-step-label"
            :class="{ 'text-positive': s.number < currentStep, 'text-primary': s.number === currentStep }"
          >{{ s.label }}</span>
        </button>
      </div>
    </q-card>

    <!-- Step content -->
    <q-card flat bordered class="wizard-content-card">
      <div class="step-title">{{ currentStepMeta.title }}</div>
      <div class="step-subtitle">{{ currentStepMeta.subtitle }}</div>
      <q-separator class="q-my-md" />

      <!-- Step 1 — Family & Studio Information -->
      <q-form v-show="currentStep === 1" :ref="(el) => (stepForms[1] = el)" greedy>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <app-select v-model="form.heardAbout" label="How Did You Hear About Us?" :options="HEARD_ABOUT_OPTIONS" />
          </div>
          <div class="col-12 col-md-6">
            <app-text-field v-model="form.referralName" label="Referral Name" placeholder="Friend or student name (optional)" />
          </div>
          <div class="col-12 col-md-6">
            <app-select v-model="form.studioLocationId" label="Studio Location" required :options="locationOptions" :rules="[required]" />
          </div>
          <div class="col-12 col-md-6">
            <app-text-field v-model="form.familyName" label="Family Name" required placeholder="e.g. Miller Family" :rules="[required]" />
          </div>
        </div>

        <div class="duplicate-check-band">
          <div class="row items-center q-gutter-sm">
            <q-icon name="o_find_replace" color="primary" size="20px" />
            <span>Check for duplicate existing family accounts in database</span>
          </div>
          <q-btn flat no-caps dense color="primary" label="Run Check" @click="runDuplicateCheck" />
        </div>
      </q-form>

      <!-- Step 2 — Contact Information -->
      <q-form v-show="currentStep === 2" :ref="(el) => (stepForms[2] = el)" greedy>
        <div v-for="contact in contactSlots" :key="contact.key" class="q-mb-md">
          <div class="step-subtitle-inline">&bull; {{ contact.heading }}</div>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <app-text-field
                v-model="contact.model.firstName" label="First Name" placeholder="First Name"
                :required="contact.required" :rules="contact.required ? [required] : []"
              />
            </div>
            <div class="col-12 col-md-4">
              <app-text-field
                v-model="contact.model.lastName" label="Last Name" placeholder="Last Name"
                :required="contact.required" :rules="contact.required ? [required] : []"
              />
            </div>
            <div class="col-12 col-md-4">
              <app-select v-model="contact.model.relation" label="Type / Relation" :options="RELATION_OPTIONS" />
            </div>
            <div class="col-12 col-md-6">
              <app-text-field
                v-model="contact.model.email" label="Email" type="email" placeholder="primary@example.com"
                :required="contact.required" :rules="contact.required ? [required, emailRule] : [optionalEmail]"
              />
            </div>
            <div class="col-12 col-md-6">
              <!-- A compound dial-code + number control, so it needs a full half-row rather than the
                   simple fields' narrower share (see AppPhoneInput's fixed-percentage code column) —
                   squeezed any narrower, the dial code's country name truncates and overlaps the number. -->
              <app-phone-input v-model="contact.model.phone" v-model:country="contact.model.phoneCountry" label="Cell Phone" :required="contact.required" />
            </div>
          </div>
          <q-checkbox v-if="contact.key === 'primary'" v-model="contact.model.textOptIn" label="Enable Text Opt-In" color="primary" dense class="q-mt-xs" />
        </div>
      </q-form>

      <!-- Step 3 — Address & Emergency Details -->
      <q-form v-show="currentStep === 3" :ref="(el) => (stepForms[3] = el)" greedy>
        <div class="row q-col-gutter-md">
          <div class="col-12">
            <app-text-field v-model="form.streetAddress" label="Street Address" required placeholder="123 Main Street, Apt 4B" :rules="[required]" />
          </div>
          <div class="col-12 col-md-4">
            <app-text-field v-model="form.city" label="City" required placeholder="Chicago" :rules="[required]" />
          </div>
          <div class="col-12 col-md-4">
            <app-text-field v-model="form.state" label="State" required placeholder="IL" :rules="[required]" />
          </div>
          <div class="col-12 col-md-4">
            <app-text-field v-model="form.postalCode" label="ZIP Code" required placeholder="60601" :rules="[required]" />
          </div>
        </div>

        <q-separator class="q-my-md" />
        <div class="step-subtitle-inline">&bull; Emergency Contact &amp; Health Insurance</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <app-text-field v-model="form.emergencyContactName" label="Emergency Contact Person" required placeholder="Full name &amp; relationship" :rules="[required]" />
          </div>
          <div class="col-12 col-md-6">
            <app-phone-input v-model="form.emergencyPhone" v-model:country="form.emergencyPhoneCountry" label="Emergency Phone" required />
          </div>
          <div class="col-12">
            <app-text-field v-model="form.insuranceCarrier" label="Health Insurance Carrier / Policy #" placeholder="e.g. BlueCross BlueShield #9823471" />
          </div>
        </div>
      </q-form>

      <!-- Step 4 — Student Profile(s) -->
      <q-form v-show="currentStep === 4" :ref="(el) => (stepForms[4] = el)" greedy>
        <div v-for="(student, index) in form.students" :key="student.key" class="q-mb-md">
          <div class="row items-center justify-between">
            <div class="step-subtitle-inline">&bull; Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}</div>
            <q-btn
              v-if="form.students.length > 1" flat dense no-caps color="negative" icon="o_close" label="Remove"
              @click="removeStudent(index)"
            />
          </div>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <app-text-field v-model="student.firstName" label="First Name" required placeholder="Student First Name" :rules="[required]" />
            </div>
            <div class="col-12 col-md-4">
              <app-text-field v-model="student.lastName" label="Last Name" required placeholder="Student Last Name" :rules="[required]" />
            </div>
            <div class="col-12 col-md-4">
              <app-text-field
                v-model="student.email" label="Student Email" type="email" required placeholder="student@example.com"
                :rules="[required, emailRule]"
                hint="A separate login account is created for the student with this email."
              />
            </div>
            <div class="col-12 col-md-4">
              <app-date-field v-model="student.birthDate" label="Birth Date" required :rules="[required]" />
            </div>
            <div class="col-12 col-md-4">
              <app-select v-model="student.gender" label="Gender" :options="GENDER_OPTIONS" />
            </div>
            <div class="col-12 col-md-4">
              <app-select v-model="student.tshirtSize" label="T-Shirt Size" :options="TSHIRT_SIZE_OPTIONS" />
            </div>
            <div class="col-12 col-md-4">
              <app-text-field v-model="student.gradeLevel" label="Grade Level" placeholder="e.g. 3rd Grade" />
            </div>
            <div class="col-12">
              <app-text-field
                v-model="student.medicalNotes" label="Allergies, Special Needs &amp; Medical Notes"
                type="textarea" autogrow placeholder="List any peanut allergies, asthma or medical requirements..."
              />
            </div>
          </div>
          <q-separator v-if="index < form.students.length - 1" class="q-mt-md" />
        </div>

        <q-btn flat no-caps dense color="primary" icon="o_add" label="Add Another Student" @click="addStudent" />
      </q-form>

      <!-- Step 5 — Class Enrollment -->
      <q-form v-show="currentStep === 5" :ref="(el) => (stepForms[5] = el)" greedy>
        <div class="enrollment-band">
          <div class="row items-center justify-between q-mb-sm">
            <span class="text-weight-bold">Class Selection</span>
          </div>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-6">
              <app-select v-model="selectedClassId" label="Choose Class" required :options="classOptions" :rules="[required]" />
            </div>
            <div class="col-12 col-md-4">
              <app-date-field v-model="form.enrollmentDate" label="Enrollment Date" />
            </div>
          </div>
          <div class="row items-center q-gutter-lg q-mt-xs">
            <q-checkbox v-model="form.trialEnrollment" label="Trial Enrollment" color="primary" dense />
            <q-checkbox v-model="form.emailInstructorNotice" label="Email Instructor Notice" color="primary" dense />
          </div>
        </div>
      </q-form>

      <!-- Step 6 — Payment & Review -->
      <q-form v-show="currentStep === 6" :ref="(el) => (stepForms[6] = el)" greedy>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-4">
            <app-select v-model="form.paymentMethod" label="Payment Method" :options="PAYMENT_METHOD_OPTIONS" :clearable="false" />
          </div>
          <div v-if="form.paymentMethod !== 'invoice'" class="col-12 col-md-8">
            <!-- The gateway's own hosted fields mount here once configured, so raw card data never
                 reaches this form or our servers. -->
            <app-field-label label="Card Details" />
            <div class="payment-slot">
              <q-icon name="o_lock" size="18px" class="q-mr-sm" />
              <span>Card details are entered in the payment gateway's secure fields, which mount here once the gateway is configured.</span>
            </div>
          </div>
        </div>

        <div class="due-today-band">
          <div class="row justify-between text-weight-bold">
            <span>Due Today (Tuition + Reg Fee):</span>
            <span>Calculated once billing is configured</span>
          </div>
          <span class="text-caption">An automated email receipt will be sent to the primary contact once payment processing is enabled.</span>
        </div>

        <q-checkbox
          v-model="form.acceptPolicies" color="primary" dense class="q-mt-md"
          label="I accept the registration fee policies and authorization rules."
        />
      </q-form>

      <q-separator class="q-my-md" />
      <div class="row items-center justify-between">
        <q-btn v-if="currentStep > 1" outline no-caps color="grey-8" icon="o_arrow_back" label="Back" @click="prevStep" />
        <div v-else />
        <div class="row q-gutter-sm">
          <q-btn flat no-caps color="grey-8" label="Cancel" :to="{ name: 'dashboard' }" />
          <q-btn
            unelevated no-caps color="primary" :loading="saving"
            :label="currentStep === STEPS.length ? 'Register Family' : 'Continue'"
            :icon-right="currentStep === STEPS.length ? undefined : 'o_arrow_forward'"
            @click="nextStep"
          />
        </div>
      </div>
    </q-card>

    <!-- Temporary passwords for every login minted by this registration (each contact, then the
         student) — up to three accounts, so a single-password dialog no longer fits. -->
    <q-dialog v-model="tempPwOpen" persistent @update:model-value="(open) => !open && onTempPasswordsClosed()">
      <q-card style="min-width: 420px;">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_key" color="primary" size="sm" />
          <div class="text-h6">Temporary passwords</div>
        </q-card-section>
        <q-card-section>
          <div class="text-body2 text-grey-7 q-mb-sm">These will not be shown again. Share them securely.</div>
          <div v-for="p in tempPasswords" :key="p.email" class="q-mb-sm">
            <div class="text-caption text-grey-7">{{ p.label }} — {{ p.email }}</div>
            <q-input :model-value="p.password" readonly outlined dense>
              <template #append>
                <q-btn flat round dense icon="o_content_copy" @click="copyPassword(p.password)">
                  <q-tooltip>Copy</q-tooltip>
                </q-btn>
              </template>
            </q-input>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn v-close-popup flat no-caps color="primary" label="Done" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";

// import AppBreadcrumbs from "components/common/AppBreadcrumbs.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppPhoneInput from "components/common/AppPhoneInput.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";

import { useNotify } from "composables/useNotify";
import { classApi, locationApi, familyApi, studentApi, getApiErrorMessage } from "services/api";
import {
  blankQuickRegistrationForm,
  blankStudent,
  toCreateFamilyRequest,
  toCreateStudentRequest,
  HEARD_ABOUT_OPTIONS,
  RELATION_OPTIONS,
  GENDER_OPTIONS,
  TSHIRT_SIZE_OPTIONS,
  PAYMENT_METHOD_OPTIONS
} from "composables/quickRegistrationForm";

const notify = useNotify();
const router = useRouter();

// Six steps, one visible at a time — the tracker card above mirrors the prototype's step badges
// (done = check, current = filled, upcoming = outline), each wired to its own q-form so "Continue"
// only validates the step being left, not the whole wizard.
const STEPS = [
  { number: 1, label: "Family Info", title: "Step 1: Family & Studio Information", subtitle: "Select studio branch, acquisition channel and primary family name." },
  { number: 2, label: "Contacts", title: "Step 2: Contact Information", subtitle: "Enter primary and secondary guardian contact details." },
  { number: 3, label: "Address", title: "Step 3: Address & Emergency Details", subtitle: "Physical residence address and medical emergency contact." },
  { number: 4, label: "Student(s)", title: "Step 4: Student Profile", subtitle: "Provide dancer information, medical considerations, and apparel sizes. Add more than one to enrol siblings together." },
  { number: 5, label: "Enrollment", title: "Step 5: Class Enrollment", subtitle: "Assign a class schedule to every student added above." },
  { number: 6, label: "Payment", title: "Step 6: Payment Schedule & Verification", subtitle: "Secure payment setup and registration finalization." }
];

const currentStep = ref(1);
const currentStepMeta = computed(() => STEPS.find((s) => s.number === currentStep.value));
const fillPercent = computed(() => ((currentStep.value - 1) / (STEPS.length - 1)) * 100);

// One q-form ref per step (siblings, not nested) so validating "Continue" only checks the fields on
// the step being left.
const stepForms = reactive({});

const goToStep = (n) => {
  // Freely revisit a completed step; skipping ahead has to go through Continue's validation.
  if (n <= currentStep.value) currentStep.value = n;
};
const prevStep = () => {
  if (currentStep.value > 1) currentStep.value -= 1;
};

const saving = ref(false);
const form = reactive(blankQuickRegistrationForm());

// The prototype repeats the same five fields for a primary and a second contact; both now save for
// real (FamiliesController.Create) — only the first is mandatory.
const contactSlots = computed(() => [
  { key: "primary", heading: "Contact #1 (Primary)", model: form.primaryContact, required: true },
  { key: "secondary", heading: "Contact #2 (Secondary / Optional)", model: form.secondaryContact, required: false }
]);

// Step 4 — one or more students, sharing the one family/class enrollment being registered.
const addStudent = () => { form.students.push(blankStudent()); };
const removeStudent = (index) => { form.students.splice(index, 1); };
const studentDisplayName = (student) => `${student.firstName || ""} ${student.lastName || ""}`.trim();

// Real classes and studio locations, fetched the same way Class's own Category dropdowns load — the
// composable's CLASS_OPTIONS were prototype demo labels, not ids a real enrollment could use.
const classes = ref([]);
const selectedClassId = ref(null);
const classOptions = computed(() => classes.value.map((c) => ({ label: c.className, value: c.classId })));
const locations = ref([]);
const locationOptions = computed(() => locations.value.map((l) => ({ label: l.name, value: l.id })));
onMounted(async () => {
  try {
    const [classResult, locationResult] = await Promise.all([
      classApi.list({ limit: 100, active: true }),
      locationApi.list({ limit: 100, active: true })
    ]);
    classes.value = classResult?.data || [];
    locations.value = locationResult?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

const tempPwOpen = ref(false);
const tempPasswords = ref([]);
const copyPassword = async (password) => {
  try {
    await navigator.clipboard.writeText(password);
    notify.success("Copied to clipboard.");
  } catch {
    notify.warning("Copy failed — please select and copy manually.");
  }
};

const required = (val) => (val !== null && val !== undefined && String(val).trim() !== "") || "Required";
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const emailRule = (val) => EMAIL_RE.test(String(val || "")) || "Enter a valid email";
const optionalEmail = (val) => !String(val || "").trim() || emailRule(val);

// Runs the real Family list search by name — the closest thing to a duplicate check without a
// dedicated endpoint.
const runDuplicateCheck = async () => {
  const name = form.familyName.trim();
  if (!name) {
    notify.warning("Enter a family name first.");
    return;
  }
  try {
    const result = await familyApi.list({ search: name, limit: 5 });
    const matches = result?.data || [];
    notify.info(
      matches.length
        ? `${matches.length} existing famil${matches.length === 1 ? "y matches" : "ies match"} "${name}".`
        : `No existing family matches "${name}".`
    );
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

// AppPhoneInput has no `rules` prop and never registers with the surrounding q-form (see its
// `validate()`, which only checks format, not presence) — required phone fields are checked here
// by hand rather than through stepForms[n].validate().
const REQUIRED_PHONE_BY_STEP = {
  2: () => (form.primaryContact.phone ? "" : "Cell Phone is required for the primary contact."),
  3: () => (form.emergencyPhone ? "" : "Emergency Phone is required.")
};

const nextStep = async () => {
  const isLastStep = currentStep.value === STEPS.length;
  const valid = await stepForms[currentStep.value]?.validate();
  if (!valid) return;

  const phoneError = REQUIRED_PHONE_BY_STEP[currentStep.value]?.();
  if (phoneError) {
    notify.warning(phoneError);
    return;
  }

  if (!isLastStep) {
    currentStep.value += 1;
    return;
  }

  // QCheckbox has no `rules` prop, so the accept-policies checkbox is checked by hand rather than
  // through the step's q-form.
  if (!form.acceptPolicies) {
    notify.warning("You must accept the registration fee policies to continue.");
    return;
  }

  // Every student's own login email has to be distinct from the other students' AND from both
  // contacts' — checked here, before any API call, the same way the two contacts' emails are
  // checked against each other back in Step 2/3's own validation.
  const allEmails = [
    form.primaryContact.email,
    ...(form.secondaryContact.email ? [form.secondaryContact.email] : []),
    ...form.students.map((s) => s.email)
  ].map((e) => e.trim().toLowerCase());
  const firstDuplicate = allEmails.find((email, i) => allEmails.indexOf(email) !== i);
  if (firstDuplicate) {
    notify.warning(`"${firstDuplicate}" is used more than once — every contact and student needs its own distinct email.`);
    return;
  }

  saving.value = true;
  // Emails already taken by an existing account are caught here, before anything saves — otherwise a
  // taken student email is only found after the family (and its contact logins) already exists.
  try {
    const { inUse = [] } = await studentApi.emailsInUse(allEmails);
    if (inUse.length) {
      notify.warning(`Already used by another account: ${inUse.join(", ")}. Every contact and student needs a new email.`);
      saving.value = false;
      return;
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    saving.value = false;
    return;
  }

  // The family (+ its contacts) has to exist before a student can name it as familyId — separate API
  // calls, not one transaction, so each is caught on its own: a student-creation failure after the
  // family (or an earlier student) already saved must not read as "nothing happened."
  let family;
  try {
    family = await familyApi.create(toCreateFamilyRequest(form));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    saving.value = false;
    return;
  }

  const passwords = (family.contacts || [])
    .filter((c) => c.temporaryPassword)
    .map((c) => ({ label: `${c.firstName} ${c.lastName}`.trim() || "Contact", email: c.email, password: c.temporaryPassword }));

  try {
    // One call, one transaction (StudentsController.CreateBulk) — every student is created together
    // or, on any failure, none of them are; there's no partial-batch state to reconcile client-side.
    const payload = form.students.map((student) => toCreateStudentRequest(student, form, selectedClassId.value, family.familyId));
    const result = await studentApi.createBulk(payload);
    (result?.students || []).forEach((created, i) => {
      if (created?.temporaryPassword) {
        passwords.push({ label: studentDisplayName(form.students[i]) || "Student", email: form.students[i].email, password: created.temporaryPassword });
      }
    });

    const studentWord = form.students.length === 1 ? "student" : `${form.students.length} students`;
    // Payment has no backing entity yet — flagged here rather than silently dropped.
    notify.success(
      form.paymentMethod === "invoice"
        ? `Family and ${studentWord} registered.`
        : `Family and ${studentWord} registered. Payment wasn't saved — there's no billing intake for it yet.`
    );

    if (passwords.length) {
      tempPasswords.value = passwords;
      tempPwOpen.value = true;
    } else {
      router.push({ name: "students" });
    }
  } catch (err) {
    // The family (+ its contacts) DID save — the student batch is the only part that didn't, and
    // none of it did (CreateBulk is all-or-nothing), so any contact logins minted above still show.
    notify.error(`Family "${form.familyName}" was created, but the student(s) could not be registered: ${getApiErrorMessage(err)}`);
    if (passwords.length) {
      tempPasswords.value = passwords;
      tempPwOpen.value = true;
    }
  } finally {
    saving.value = false;
  }
};

// The temp-password dialog is the last thing the user sees for a new registration (same pattern as
// the Students page) — leaving the registration page only happens once it's dismissed.
const onTempPasswordsClosed = () => {
  router.push({ name: "students" });
};
</script>

<style scoped>
.wizard-page-title {
  font-size: 26px;
  font-weight: 700;
  color: var(--on-surface);
  line-height: 1.2;
}
.wizard-page-subtitle {
  font-size: 13px;
  color: var(--outline);
  margin-bottom: 16px;
}

.wizard-tracker-card {
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 16px;
}
.wizard-tracker {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  position: relative;
  max-width: 800px;
  margin: 0 auto;
}
.wizard-tracker__line,
.wizard-tracker__line-fill {
  position: absolute;
  top: 16px;
  left: 16px;
  right: 16px;
  height: 2px;
  z-index: 0;
}
.wizard-tracker__line {
  background: var(--outline-variant, #e0e0e0);
}
.wizard-tracker__line-fill {
  right: auto;
  background: var(--q-primary);
  transition: width 0.25s ease;
}
.wizard-step-node {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  background: none;
  border: none;
  padding: 0;
  position: relative;
  z-index: 1;
  cursor: pointer;
}
.wizard-step-node:disabled {
  cursor: default;
}
.wizard-step-badge {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  background: var(--surface, #fff);
  border: 2px solid var(--outline-variant, #e0e0e0);
  color: var(--outline, #9e9e9e);
}
.wizard-step-badge--done {
  background: var(--q-positive);
  border-color: var(--q-positive);
  color: #fff;
}
.wizard-step-badge--active {
  background: var(--q-primary);
  border-color: var(--q-primary);
  color: #fff;
}
.wizard-step-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--outline, #9e9e9e);
  white-space: nowrap;
}

.wizard-content-card {
  border-radius: 16px;
  padding: 24px;
  max-width: 900px;
  margin: 0 auto;
}
.step-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--on-surface);
}
.step-subtitle {
  font-size: 12px;
  color: var(--outline);
  margin-top: 2px;
}
.step-subtitle-inline {
  font-size: 12px;
  font-weight: 700;
  color: var(--q-primary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 8px;
}

.duplicate-check-band,
.enrollment-band {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--surface-container-low, #f5f5f7);
  border: 1px solid var(--outline-variant, #e0e0e0);
  border-radius: 12px;
  padding: 12px 16px;
  margin-top: 16px;
  font-size: 12px;
}
.enrollment-band {
  display: block;
  background: var(--surface-container-low, #f5f5f7);
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

.due-today-band {
  display: flex;
  flex-direction: column;
  gap: 4px;
  background: rgba(76, 175, 80, 0.08);
  border: 1px solid rgba(76, 175, 80, 0.35);
  color: #2e7d32;
  border-radius: 12px;
  padding: 12px 16px;
  margin-top: 16px;
  font-size: 13px;
}
</style>
