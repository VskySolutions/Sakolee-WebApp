<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Families' }]"
      title="All Families"
      description="Manage student families and parent records."
      :search="search"
      show-search
      search-placeholder="Search family name"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Family"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.familyStatusId" label="Family Status" :options="familyStatusOptions" />
    </app-filter-drawer>

    <app-data-table
      page-key="families"
      row-key="familyId"
      title="All Families"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn v-if="canWrite" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit / View drawer -->
    <app-form-drawer
      v-model="formOpen"
      :title="editing ? 'Edit Family' : 'Create Family'"
      :saving="saving"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-tabs v-model="activeTab" dense no-caps align="justify" class="text-grey-7" active-color="primary" indicator-color="primary">
        <q-tab name="family" label="Family Info" />
        <q-tab name="contacts" label="Contacts" />
        <q-tab name="address" label="Address & Emergency" />
        <q-tab name="students" label="Students" />
      </q-tabs>
      <q-separator class="q-mb-md" />

      <!-- All four tabs' fields stay mounted (toggled with v-show, not v-if) so the single q-form
           below can validate every field regardless of which tab is showing — an unmounted required
           field in an unvisited tab would silently pass validation otherwise. -->
      <q-form ref="formRef" greedy>
        <div v-show="activeTab === 'family'" class="row q-col-gutter-md">
          <app-text-field v-model="form.familyName" label="Family Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Family name is required']" />
          <app-select v-model="form.studioLocationId" label="Studio Location" :options="locationOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.familyStatusId" label="Family Status" :options="familyStatusOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.source" label="How Did You Hear About Us?" :options="sourceOptions" class="col-12 col-sm-6" />
          <app-text-field v-model="form.referralName" label="Referral Name" class="col-12 col-sm-6" />

          <div v-if="editing" class="col-12">
            <q-toggle v-model="form.active" label="Active" />
          </div>
        </div>

        <div v-show="activeTab === 'contacts'" class="row q-col-gutter-md">
          <div class="col-12 text-subtitle2 text-grey-8">Contact #1 (Primary)</div>
          <app-text-field v-model="form.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required']" />
          <app-text-field v-model="form.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required']" />
          <app-select v-model="form.relation" label="Relation" :options="relationOptions" class="col-12 col-sm-6" />
          <app-text-field
            v-model="form.email" label="Email" type="email" required class="col-12 col-sm-6"
            :error="!!primaryEmailError" :error-message="primaryEmailError"
            :rules="[(v) => !!v || 'Email is required']"
            hint="A login account is created for this contact."
          />
          <app-text-field v-model="form.homePhone" label="Home Phone" class="col-12 col-sm-6" />
          <app-text-field v-model="form.workPhone" label="Work Phone" class="col-12 col-sm-6" />
          <app-text-field v-model="form.cellPhone" label="Cell Phone" class="col-12 col-sm-6" />
          <app-text-field v-model="form.otherPhone" label="Other Phone" class="col-12 col-sm-6" />
          <app-text-field v-model="form.fax" label="Fax" class="col-12 col-sm-6" />
          <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.isBillingContact" color="primary" /><span class="q-ml-sm">Billing contact</span></div>
          <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.isAuthorizedToPickUpStudent" color="primary" /><span class="q-ml-sm">Authorized to pick up student</span></div>

          <div class="col-12 row items-center justify-between q-mt-sm">
            <div class="text-subtitle2 text-grey-8">Contact #2 (Secondary / Optional)</div>
            <q-btn v-if="!hasSecondaryContact" flat dense no-caps color="primary" icon="o_add" label="Add second contact" @click="addSecondaryContact" />
            <q-btn v-else-if="hasSecondaryContact && !editing" flat dense no-caps color="negative" icon="o_close" label="Remove" @click="removeSecondaryContact" />
          </div>
          <template v-if="hasSecondaryContact">
            <app-text-field v-model="form.secondaryContact.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required']" />
            <app-text-field v-model="form.secondaryContact.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required']" />
            <app-select v-model="form.secondaryContact.relation" label="Relation" :options="relationOptions" class="col-12 col-sm-6" />
            <app-text-field
              v-model="form.secondaryContact.email" label="Email" type="email" required class="col-12 col-sm-6"
              :error="!!secondaryEmailError" :error-message="secondaryEmailError"
              :rules="[(v) => !!v || 'Email is required']"
              hint="A login account is created for this contact."
            />
            <app-text-field v-model="form.secondaryContact.phone" label="Cell Phone" class="col-12 col-sm-6" />
            <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.secondaryContact.isBillingContact" color="primary" /><span class="q-ml-sm">Billing contact</span></div>
            <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.secondaryContact.isAuthorizedToPickUpStudent" color="primary" /><span class="q-ml-sm">Authorized to pick up student</span></div>
          </template>
        </div>

        <div v-show="activeTab === 'address'" class="row q-col-gutter-md">
          <div class="col-12 text-subtitle2 text-grey-8">Household Address</div>
          <app-text-field v-model="form.address1" label="Street Address" class="col-12" />
          <app-text-field v-model="form.address2" label="Street Address 2" class="col-12" />
          <app-text-field v-model="form.city" label="City" class="col-12 col-sm-4" />
          <app-text-field v-model="form.state" label="State" class="col-12 col-sm-4" />
          <app-text-field v-model.number="form.zipCode" label="ZIP Code" type="number" class="col-12 col-sm-4" />

          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Emergency Contact &amp; Health Insurance</div>
          <app-text-field v-model="form.emergencyContactPerson" label="Emergency Contact Person" class="col-12 col-sm-6" />
          <app-text-field v-model="form.emergencyPhone" label="Emergency Phone" class="col-12 col-sm-6" />
          <app-text-field v-model="form.healthInsuranceCarrier" label="Health Insurance Carrier / Policy #" class="col-12" />
        </div>

        <div v-show="activeTab === 'students'" class="row q-col-gutter-md">
          <template v-if="students.length">
            <div class="col-12 text-subtitle2 text-grey-8">Enrolled Students</div>
            <div class="col-12">
              <q-list bordered separator>
                <q-item v-for="s in students" :key="s.studentId">
                  <q-item-section>
                    <q-item-label>{{ s.firstName || s.lastName ? `${s.firstName || ''} ${s.lastName || ''}`.trim() : (s.studentNumber || 'Student') }}</q-item-label>
                    <q-item-label caption>{{ s.studentNumber || '—' }}</q-item-label>
                  </q-item-section>
                  <q-item-section side class="row items-center q-gutter-xs">
                    <q-badge :color="s.active ? 'positive' : 'grey'">{{ s.active ? 'Active' : 'Inactive' }}</q-badge>
                    <q-btn flat round dense color="primary" icon="o_edit" @click="openStudentEdit(s.studentId)">
                      <q-tooltip>Edit student</q-tooltip>
                    </q-btn>
                  </q-item-section>
                </q-item>
              </q-list>
            </div>
          </template>

          <!-- Add one or more new students under this family — mirrors Quick Registration's Step 4,
               reusing the same blankStudent()/studentApi.createBulk() so both entry points behave the
               same way. Not shown in read-only View mode. -->
          <template v-if="editing">
            <div class="col-12 row items-center justify-between q-mt-sm">
              <div class="text-subtitle2 text-grey-8">Add Student(s)</div>
              <q-btn flat dense no-caps color="primary" icon="o_add" label="Add Student" @click="addNewStudent" />
            </div>
            <div v-for="(student, index) in form.newStudents" :key="student.key" class="col-12">
              <div class="row items-center justify-between q-mb-xs">
                <div class="text-caption text-weight-bold text-primary">
                  Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}
                </div>
                <q-btn flat dense no-caps color="negative" icon="o_close" label="Remove" @click="removeNewStudent(index)" />
              </div>
              <div class="row q-col-gutter-md q-mb-sm">
                <app-text-field v-model="student.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required']" />
                <app-text-field v-model="student.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required']" />
                <app-text-field
                  v-model="student.email" label="Student Email" type="email" required class="col-12 col-sm-6"
                  :rules="[(v) => !!v || 'Email is required']"
                  hint="A separate login account is created for the student with this email."
                />
                <app-date-field v-model="student.birthDate" label="Birth Date" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Birth date is required']" />
                <app-select v-model="student.gender" label="Gender" :options="genderOptions" class="col-12 col-sm-6" />
                <app-select v-model="student.tshirtSize" label="T-Shirt Size" :options="tshirtSizeOptions" class="col-12 col-sm-6" />
                <app-text-field v-model="student.gradeLevel" label="Grade Level" class="col-12 col-sm-6" />
                <app-text-field v-model="student.medicalNotes" label="Allergies, Special Needs &amp; Medical Notes" type="textarea" class="col-12" />
              </div>
              <q-separator v-if="index < form.newStudents.length - 1" />
            </div>
          </template>

          <div v-if="!students.length && !form.newStudents.length" class="col-12 text-grey-6 text-caption">
            No students enrolled yet.
          </div>
        </div>
      </q-form>
    </app-form-drawer>

    <student-edit-dialog v-model="studentEditOpen" :student-id="studentEditId" @saved="onStudentSaved" />

    <!-- Temporary passwords for newly-created contact logins -->
    <q-dialog v-model="tempPwOpen" persistent>
      <q-card style="min-width: 420px;">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_key" color="primary" size="sm" />
          <div class="text-h6">Temporary passwords</div>
        </q-card-section>
        <q-card-section>
          <div class="text-body2 text-grey-7 q-mb-sm">These will not be shown again. Share them with each contact securely.</div>
          <div v-for="c in tempPasswords" :key="c.email" class="q-mb-sm">
            <div class="text-caption text-grey-7">{{ c.email }}</div>
            <q-input :model-value="c.password" readonly outlined dense>
              <template #append>
                <q-btn flat round dense icon="o_content_copy" @click="copyPassword(c.password)">
                  <q-tooltip>Copy</q-tooltip>
                </q-btn>
              </template>
            </q-input>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn v-close-popup flat no-caps color="primary" label="Done" @click="onTempPasswordsClosed" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <app-form-drawer
      v-model="viewFormOpen"
      title="View Family"
      hide-save
      @cancel="resetViewForm"
    >
      <div class="row q-col-gutter-lg">

        <!-- Family Information -->
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Family Information
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Family Name</div>
          <div class="text-2e fs-14">
            {{ form.familyName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Studio Location</div>
          <div class="text-2e fs-14">
            {{ form.studioLocationName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Family Status</div>
          <div class="text-2e fs-14">
            {{
              familyStatusOptions.find(
                (x) => x.value === form.familyStatusId
              )?.label || "—"
            }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">How Did You Hear About Us?</div>
          <div class="text-2e fs-14">
            {{ form.source || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Referral Name</div>
          <div class="text-2e fs-14">
            {{ form.referralName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500 q-mb-xs">Status</div>

          <q-badge :color="form.active ? 'positive' : 'grey'">
            {{ form.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <!-- Primary Contact -->
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Primary Contact
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">First Name</div>
          <div class="text-2e fs-14">
            {{ form.firstName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Last Name</div>
          <div class="text-2e fs-14">
            {{ form.lastName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Relation</div>
          <div class="text-2e fs-14">
            {{ form.relation || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Email</div>
          <div class="text-2e fs-14">
            {{ form.email || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Home Phone</div>
          <div class="text-2e fs-14">
            {{ form.homePhone || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Work Phone</div>
          <div class="text-2e fs-14">
            {{ form.workPhone || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Cell Phone</div>
          <div class="text-2e fs-14">
            {{ form.cellPhone || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Other Phone</div>
          <div class="text-2e fs-14">
            {{ form.otherPhone || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Fax</div>
          <div class="text-2e fs-14">
            {{ form.fax || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Billing Contact</div>
          <div class="text-2e fs-14">
            {{ form.isBillingContact ? "Yes" : "No" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Authorized to Pick Up Student
          </div>
          <div class="text-2e fs-14">
            {{ form.isAuthorizedToPickUpStudent ? "Yes" : "No" }}
          </div>
        </div>

        <!-- Secondary Contact -->
        <template v-if="form.secondaryContact">
          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
            Secondary Contact
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">First Name</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.firstName || "—" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">Last Name</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.lastName || "—" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">Relation</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.relation || "—" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">Email</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.email || "—" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">Cell Phone</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.phone || "—" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">Billing Contact</div>
            <div class="text-2e fs-14">
              {{ form.secondaryContact.isBillingContact ? "Yes" : "No" }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <div class="text-86 fs-12 fw-500">
              Authorized to Pick Up Student
            </div>
            <div class="text-2e fs-14">
              {{
                form.secondaryContact.isAuthorizedToPickUpStudent
                  ? "Yes"
                  : "No"
              }}
            </div>
          </div>
        </template>

        <!-- Household Address -->
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Household Address
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Street Address</div>
          <div class="text-2e fs-14">
            {{ form.address1 || "—" }}
          </div>
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Street Address 2</div>
          <div class="text-2e fs-14">
            {{ form.address2 || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-4">
          <div class="text-86 fs-12 fw-500">City</div>
          <div class="text-2e fs-14">
            {{ form.city || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-4">
          <div class="text-86 fs-12 fw-500">State</div>
          <div class="text-2e fs-14">
            {{ form.state || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-4">
          <div class="text-86 fs-12 fw-500">ZIP Code</div>
          <div class="text-2e fs-14">
            {{ form.zipCode || "—" }}
          </div>
        </div>

        <!-- Emergency / Insurance -->
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Emergency Contact & Health Insurance
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Emergency Contact Person
          </div>
          <div class="text-2e fs-14">
            {{ form.emergencyContactPerson || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Emergency Phone</div>
          <div class="text-2e fs-14">
            {{ form.emergencyPhone || "—" }}
          </div>
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">
            Health Insurance Carrier / Policy #
          </div>
          <div class="text-2e fs-14">
            {{ form.healthInsuranceCarrier || "—" }}
          </div>
        </div>

        <!-- Students -->
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Enrolled Students
        </div>

        <div v-if="students.length" class="col-12">
          <q-list bordered separator>
            <q-item
              v-for="student in students"
              :key="student.studentId"
            >
              <q-item-section>
                <q-item-label>
                  {{
                    student.firstName || student.lastName
                      ? `${student.firstName || ""} ${student.lastName || ""}`.trim()
                      : student.studentNumber || "Student"
                  }}
                </q-item-label>

                <q-item-label caption>
                  {{ student.studentNumber || "—" }}
                </q-item-label>
              </q-item-section>

              <q-item-section side>
                <q-badge :color="student.active ? 'positive' : 'grey'">
                  {{ student.active ? "Active" : "Inactive" }}
                </q-badge>
              </q-item-section>
            </q-item>
          </q-list>
        </div>

        <div v-else class="col-12 text-grey-6 text-caption">
          No students enrolled yet.
        </div>

      </div>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";
import { familyApi, locationApi, familyStatusApi, studentApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { usePermissions, Permissions } from "composables/usePermissions";
import { RELATION_OPTIONS, HEARD_ABOUT_OPTIONS, GENDER_OPTIONS, TSHIRT_SIZE_OPTIONS, blankStudent } from "composables/quickRegistrationForm";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppDateField from "components/common/AppDateField.vue";
import StudentEditDialog from "modules/family/components/StudentEditDialog.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { has } = usePermissions();
const canWrite = computed(() => has(Permissions.FamiliesWrite));
const canDelete = computed(() => has(Permissions.FamiliesDelete));

const columns = [
  { name: "familyName", label: "Family Name", field: "familyName", align: "left", default: true, sortable: true },
  { name: "primaryContactName", label: "Primary Contact", field: (row) => row.primaryContactName || "—", align: "left", default: true },
  { name: "primaryContactEmail", label: "Email", field: (row) => row.primaryContactEmail || "—", align: "left", default: true },
  { name: "primaryContactPhone", label: "Phone", field: (row) => row.primaryContactPhone || "—", align: "left" },
  { name: "familyStatusName", label: "Status", field: (row) => row.familyStatusName || "—", align: "left" },
  { name: "studioLocationName", label: "Studio Location", field: (row) => row.studioLocationName || "—", align: "left" },
  { name: "studentCount", label: "Students", field: "studentCount", align: "left" },
  { name: "active", label: "Active", field: "active", align: "left", default: true },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

// ---- Reference option lists ----
const locations = ref([]);
const familyStatuses = ref([]);
// Same list as the Class form's Location. A family whose saved location has since been deactivated
// still shows it by name (from the family record) rather than as a raw id.
const locationOptions = computed(() => {
  const options = locations.value.map((l) => ({ label: l.name, value: l.id }));
  if (form.studioLocationId && form.studioLocationName && !options.some((o) => o.value === form.studioLocationId)) {
    options.unshift({ label: form.studioLocationName, value: form.studioLocationId });
  }
  return options;
});
// FamilyStatusSummary's id field is just "id" (Guid Id), not "familyStatusId" — mirrors the
// familystatus module's own pages, which fall back through the same mismatch.
const familyStatusOptions = computed(() => familyStatuses.value.map((s) => ({ label: s.name, value: s.id })));
const relationOptions = RELATION_OPTIONS;
const sourceOptions = HEARD_ABOUT_OPTIONS;
const genderOptions = GENDER_OPTIONS;
const tshirtSizeOptions = TSHIRT_SIZE_OPTIONS;

onMounted(async () => {
  try {
    const [locRes, statusRes] = await Promise.all([
      locationApi.list({ limit: 100, active: true }),
      familyStatusApi.list({ limit: 100 })
    ]);
    locations.value = locRes?.data || [];
    familyStatuses.value = statusRes?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

const filters = reactive({ familyStatusId: null });
const { rows, loading, totalRecords, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "families",
  fetcher: ({ page, limit, sortBy, descending }) =>
    familyApi.list({
      page,
      limit,
      sortBy,
      descending,
      familyStatusId: filters.familyStatusId || undefined,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const filterChips = computed(() => {
  const chips = [];
  if (filters.familyStatusId) {
    const found = familyStatusOptions.value.find((o) => o.value === filters.familyStatusId);
    chips.push({ key: "familyStatusId", label: `Status: ${found ? found.label : filters.familyStatusId}` });
  }
  return chips;
});
const removeFilter = (key) => { if (key === "familyStatusId") filters.familyStatusId = null; };
const clearFilters = () => { filters.familyStatusId = null; };

// ---- Create / Edit / View ----
const formOpen = ref(false);
const editing = ref(false);
// const viewing = ref(false);
const viewFormOpen = ref(false);
const saving = ref(false);
const primaryEmailError = ref("");
const secondaryEmailError = ref("");
const formRef = ref(null);
const editingFamilyId = ref(null);
const students = ref([]);
const activeTab = ref("family");

// ---- Enrolled-student edit (the only place a student's details can be edited now — see
// StudentEditDialog's remarks) ----
const studentEditOpen = ref(false);
const studentEditId = ref(null);
const openStudentEdit = (studentId) => {
  studentEditId.value = studentId;
  studentEditOpen.value = true;
};
const onStudentSaved = async () => {
  // Refresh this family's own student list so the edited name/status shows immediately.
  if (!editingFamilyId.value) return;
  try {
    const detail = await familyApi.get(editingFamilyId.value);
    students.value = detail.students || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const blankSecondaryContact = () => ({ firstName: "", lastName: "", email: "", phone: "", relation: "", isBillingContact: false, isAuthorizedToPickUpStudent: false });
const blankForm = () => ({
  familyName: "",
  studioLocationId: null,
  studioLocationName: "", // display-only, never sent
  familyStatusId: null,
  source: "",
  referralName: "",
  // Primary contact — inlined on the Family record itself (see FamiliesController remarks).
  firstName: "",
  lastName: "",
  email: "",
  relation: "",
  homePhone: "",
  workPhone: "",
  cellPhone: "",
  otherPhone: "",
  fax: "",
  isBillingContact: true,
  isAuthorizedToPickUpStudent: true,
  secondaryContact: null,
  address1: "",
  address2: "",
  city: "",
  state: "",
  zipCode: null,
  emergencyContactPerson: "",
  emergencyPhone: "",
  healthInsuranceCarrier: "",
  active: true,
  // New students to enrol under this family on save — see the "Add Student(s)" section. Never
  // pre-filled from an existing family's own students (those come back read-only via `students`,
  // populated separately below).
  newStudents: []
});
const form = reactive(blankForm());
const hasSecondaryContact = computed(() => !!form.secondaryContact);
const addSecondaryContact = () => { form.secondaryContact = blankSecondaryContact(); };
const removeSecondaryContact = () => { form.secondaryContact = null; };
const addNewStudent = () => { form.newStudents.push(blankStudent()); };
const removeNewStudent = (index) => { form.newStudents.splice(index, 1); };
const studentDisplayName = (student) => `${student.firstName || ""} ${student.lastName || ""}`.trim();

const resetForm = () => {
  Object.assign(form, blankForm());
  primaryEmailError.value = "";
  secondaryEmailError.value = "";
  editing.value = false;
  editingFamilyId.value = null;
  students.value = [];
  activeTab.value = "family";
};

const resetViewForm = () => {
  viewFormOpen.value = false;
  Object.assign(form, blankForm());
  editingFamilyId.value = null;
  students.value = [];
  activeTab.value = "family";
};

const openCreate = () => {
  resetForm();
  editing.value = false;
  formOpen.value = true;
};

const populateFrom = (detail) => {
  const primary = (detail.contacts || []).find((c) => c.isPrimaryContact) || null;
  const secondary = (detail.contacts || []).find((c) => !c.isPrimaryContact) || null;

  Object.assign(form, {
    familyName: detail.familyName || "",
    studioLocationId: detail.studioLocationId || null,
    studioLocationName: detail.studioLocationName || "",
    familyStatusId: detail.familyStatusId || null,
    source: detail.source || "",
    referralName: detail.referralName || "",
    firstName: primary?.firstName || "",
    lastName: primary?.lastName || "",
    email: primary?.email || "",
    relation: primary?.relation || "",
    homePhone: detail.homePhone || "",
    workPhone: detail.workPhone || "",
    cellPhone: primary?.phone || "",
    otherPhone: detail.otherPhone || "",
    fax: detail.fax || "",
    isBillingContact: !!primary?.isBillingContact,
    isAuthorizedToPickUpStudent: !!primary?.isAuthorizedToPickUpStudent,
    secondaryContact: secondary
      ? { firstName: secondary.firstName || "", lastName: secondary.lastName || "", email: secondary.email || "", phone: secondary.phone || "", relation: secondary.relation || "", isBillingContact: !!secondary.isBillingContact, isAuthorizedToPickUpStudent: !!secondary.isAuthorizedToPickUpStudent }
      : null,
    address1: detail.address1 || "",
    address2: detail.address2 || "",
    city: detail.city || "",
    state: detail.state || "",
    zipCode: detail.zipCode ?? null,
    emergencyContactPerson: detail.emergencyContactPerson || "",
    emergencyPhone: detail.emergencyPhone || "",
    healthInsuranceCarrier: detail.healthInsuranceCarrier || "",
    active: detail.active
  });
  students.value = detail.students || [];
};

const openEdit = async (row) => {
  resetForm();
  editing.value = true;
  editingFamilyId.value = row.familyId;
  formOpen.value = true;
  try {
    const detail = await familyApi.get(row.familyId);
    populateFrom(detail);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const openView = async (row) => {
  resetViewForm();
  editingFamilyId.value = row.familyId;
  viewFormOpen.value = true;
  try {
    const detail = await familyApi.get(row.familyId);
    populateFrom(detail);
  } catch (err) {
    viewFormOpen.value = false;
    notify.error(getApiErrorMessage(err));
  }
};

const toSecondaryContactPayload = (contact) => ({
  firstName: contact.firstName,
  lastName: contact.lastName,
  email: contact.email,
  phone: contact.phone || null,
  relation: contact.relation || null,
  isBillingContact: contact.isBillingContact,
  isAuthorizedToPickUpStudent: contact.isAuthorizedToPickUpStudent
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
const onTempPasswordsClosed = () => {
  formOpen.value = false;
  resetForm();
  load();
};

const submitForm = async () => {
  primaryEmailError.value = "";
  secondaryEmailError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) {
    // All four tabs' fields stay mounted (v-show), so validate() above already checked every one —
    // this only decides which tab to land the user on so they can actually see what failed.
    if (!form.familyName) {
      activeTab.value = "family";
    } else if (
      !form.firstName || !form.lastName || !form.email ||
      (form.secondaryContact && (!form.secondaryContact.firstName || !form.secondaryContact.lastName || !form.secondaryContact.email))
    ) {
      activeTab.value = "contacts";
    } else if (form.newStudents.some((s) => !s.firstName || !s.lastName || !s.email || !s.birthDate)) {
      activeTab.value = "students";
    }
    return;
  }

  // Every contact and every new student needs its own distinct email — checked up front, before any
  // API call, the same way Quick Registration's own submit does.
  const allEmails = [
    form.email,
    ...(form.secondaryContact?.email ? [form.secondaryContact.email] : []),
    ...form.newStudents.map((s) => s.email)
  ].map((e) => e.trim().toLowerCase());
  const firstDuplicate = allEmails.find((email, i) => allEmails.indexOf(email) !== i);
  if (firstDuplicate) {
    notify.warning(`"${firstDuplicate}" is used more than once — every contact and student needs its own distinct email.`);
    return;
  }

  const payload = {
    familyName: form.familyName,
    studioLocationId: form.studioLocationId || null,
    familyStatusId: form.familyStatusId || null,
    source: form.source || null,
    referralName: form.referralName || null,
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email,
    relation: form.relation || null,
    homePhone: form.homePhone || null,
    workPhone: form.workPhone || null,
    cellPhone: form.cellPhone || null,
    otherPhone: form.otherPhone || null,
    fax: form.fax || null,
    isBillingContact: form.isBillingContact,
    isAuthorizedToPickUpStudent: form.isAuthorizedToPickUpStudent,
    address1: form.address1 || null,
    address2: form.address2 || null,
    city: form.city || null,
    state: form.state || null,
    zipCode: form.zipCode || null,
    emergencyContactPerson: form.emergencyContactPerson || null,
    emergencyPhone: form.emergencyPhone || null,
    healthInsuranceCarrier: form.healthInsuranceCarrier || null,
    secondaryContact: form.secondaryContact ? toSecondaryContactPayload(form.secondaryContact) : null
  };

  saving.value = true;
  let result;
  try {
    if (editing.value) {
      result = await familyApi.update(editingFamilyId.value, { ...payload, active: form.active });
      notify.success("Family updated.");
    } else {
      result = await familyApi.create(payload);
      notify.success("Family created.");
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    saving.value = false;
    return;
  }

  const passwords = (result?.contacts || [])
    .filter((c) => c.temporaryPassword)
    .map((c) => ({ email: c.email, password: c.temporaryPassword }));

  // Any new students are created in one bulk call (StudentsController.CreateBulk) — the family (+ its
  // contacts) already saved above, so this is caught on its own: a failure here must not read as
  // "nothing happened."
  if (form.newStudents.length) {
    try {
      const studentPayload = form.newStudents.map((student) => ({
        familyId: result.familyId,
        familyName: form.familyName || null,
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.email,
        birthDate: student.birthDate || null,
        gender: student.gender || null,
        tShirtSize: student.tshirtSize || null,
        gradeLevel: student.gradeLevel || null,
        specialNeeds: student.medicalNotes || null,
        healthInsuranceCarrier: form.healthInsuranceCarrier || null,
        emergencyContactName: form.emergencyContactPerson || null,
        emergencyContactNumber: form.emergencyPhone || null
      }));
      const studentResult = await studentApi.createBulk(studentPayload);
      (studentResult?.students || []).forEach((created, i) => {
        if (created?.temporaryPassword) {
          passwords.push({ email: form.newStudents[i].email, password: created.temporaryPassword });
        }
      });
      notify.success(`${form.newStudents.length} student(s) enrolled.`);
    } catch (err) {
      notify.error(`Family "${form.familyName}" was saved, but the new student(s) could not be enrolled: ${getApiErrorMessage(err)}`);
    }
  }

  saving.value = false;
  if (passwords.length) {
    tempPasswords.value = passwords;
    tempPwOpen.value = true;
    return; // formOpen/resetForm/load happen once the temp-password dialog is dismissed
  }

  formOpen.value = false;
  resetForm();
  load();
};

const remove = async (row) => {
  const ok = await confirm({
    title: "Delete family",
    message: `Delete "${row.familyName}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await familyApi.remove(row.familyId);
    notify.success("Family deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>

<style scoped>
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
</style>
