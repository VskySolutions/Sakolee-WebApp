<template>
  <div class="app-menu-wrapper">

    <!-- =====================================================
         SCROLLABLE MAIN MENU
         ===================================================== -->
    <q-scroll-area class="app-menu-scroll">

      <q-list class="app-menu q-py-xs q-gutter-y-sm">

        <template
          v-for="section in visibleSections"
          :key="section.key"
        >

          <!-- =================================================
               UNGROUPED ITEMS
               ================================================= -->
          <!-- Dashboard -->
          <template v-if="!section.label">

            <q-item
              v-for="item in section.items"
              :key="item.label"
              v-ripple
              dense
              clickable
              :to="item.to"
              :exact="item.exact"
              active-class="active-menu-class"
              class="dashboard-menu"
              @click="onItem(item)"
            >
              <q-item-section avatar>
                <q-icon
                  :name="item.icon"
                  size="22px"
                />
              </q-item-section>

              <q-item-section
                class="fs-14 fw-500"
              >
                {{ item.label }}
              </q-item-section>

              <q-tooltip
                v-if="mini"
                anchor="center right"
                self="center left"
              >
                {{ item.label }}
              </q-tooltip>
            </q-item>

          </template>

          <!-- =================================================
               MINI MODE
               ================================================= -->
          <q-item
            v-else-if="mini"
            :key="`${section.key}-mini`"
            v-ripple
            dense
            clickable
            @mouseenter="openFlyout(section.key)"
            @mouseleave="scheduleFlyoutClose"
            @click="openFlyout(section.key)"
          >

            <q-item-section avatar>
              <q-icon
                :name="section.icon"
                size="22px"
              />
            </q-item-section>

            <q-item-section>
              {{ section.label }}
            </q-item-section>

            <q-menu
              :model-value="flyoutKey === section.key"
              anchor="top right"
              self="top left"
              :offset="[0, 0]"
              no-parent-event
              no-focus
              no-refocus
              transition-show="jump-right"
              transition-hide="jump-left"
              @update:model-value="
                (open) => (flyoutKey = open ? section.key : null)
              "
            >

              <q-list
                dense
                class="app-menu__flyout"
                @mouseenter="openFlyout(section.key)"
                @mouseleave="scheduleFlyoutClose"
              >

                <q-item-label
                  header
                  class="app-menu__flyout-head"
                >
                  {{ section.label }}
                </q-item-label>

                <!-- <q-item
                  v-for="item in section.items"
                  :key="item.label"
                  v-ripple
                  v-close-popup
                  dense
                  clickable
                  :to="item.to"
                  :exact="item.exact"
                  active-class="active-menu-class"
                  @click="onItem(item)"
                >

                  <q-item-section avatar>
                    <q-icon
                      :name="item.icon"
                      size="20px"
                    />
                  </q-item-section>

                  <q-item-section>
                    {{ item.label }}
                  </q-item-section>

                </q-item> -->
                <template
                  v-for="item in section.items"
                  :key="item.key || item.label"
                >
                  <!-- Direct menu item -->
                  <q-item
                    v-if="!item.items"
                    v-ripple
                    v-close-popup
                    dense
                    clickable
                    :to="item.to"
                    :exact="item.exact"
                    active-class="active-menu-class"
                    @click="onItem(item)"
                  >
                    <q-item-section avatar>
                      <q-icon
                        :name="item.icon"
                        size="20px"
                      />
                    </q-item-section>

                    <q-item-section>
                      {{ item.label }}
                    </q-item-section>
                  </q-item>

                  <!-- Nested Settings -->
                  <q-item
                    v-else
                    v-ripple
                    dense
                    clickable
                    class="app-menu__flyout-parent"
                    @mouseenter="openNestedFlyout(section.key + '-' + item.key)"
                    @mouseleave="scheduleNestedFlyoutClose"
                    @click="openNestedFlyout(section.key + '-' + item.key)"
                  >
                    <q-item-section avatar>
                      <q-icon
                        :name="item.icon"
                        size="20px"
                      />
                    </q-item-section>

                    <q-item-section>
                      {{ item.label }}
                    </q-item-section>

                    <q-item-section side>
                      <q-icon
                        name="o_chevron_right"
                        size="18px"
                      />
                    </q-item-section>

                    <q-menu
                      :model-value="
                        nestedFlyoutKey === section.key + '-' + item.key
                      "
                      anchor="top right"
                      self="top left"
                      :offset="[0, 0]"
                      no-parent-event
                      no-focus
                      no-refocus
                      transition-show="jump-right"
                      transition-hide="jump-left"
                      @update:model-value="
                        (open) =>
                          nestedFlyoutKey =
                          open ? section.key + '-' + item.key : null
                      "
                    >
                      <q-list
                        dense
                        class="app-menu__nested-flyout"
                        @mouseenter="
                          openNestedFlyout(section.key + '-' + item.key)
                        "
                        @mouseleave="scheduleNestedFlyoutClose"
                      >
                        <q-item-label
                          header
                          class="app-menu__flyout-head"
                        >
                          {{ item.label }}
                        </q-item-label>

                        <q-item
                          v-for="child in item.items"
                          :key="child.label"
                          v-ripple
                          v-close-popup
                          dense
                          clickable
                          :to="child.to"
                          :exact="child.exact"
                          active-class="active-menu-class"
                          @click="onItem(child)"
                        >
                          <q-item-section avatar>
                            <q-icon
                              :name="child.icon"
                              size="20px"
                            />
                          </q-item-section>

                          <q-item-section>
                            {{ child.label }}
                          </q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-item>
                </template>

              </q-list>

            </q-menu>

          </q-item>

          <!-- =================================================
               NORMAL MODE - EXPANSION GROUP
               ================================================= -->
          <q-expansion-item
            v-else
            dense
            :icon="section.icon"
            :label="section.label"
            :model-value="isOpen(section)"
            header-class="app-menu__group"
            class="sakolee-desktop-menu-class"
            @update:model-value="
              (v) => setOpen(section.key, v)
            "
          >

            <template
              v-for="item in section.items"
              :key="item.key || item.label"
            >
              <!-- ============================================
                  NORMAL MENU ITEM
                  ============================================ -->
              <q-item
                v-if="!item.items"
                v-ripple
                dense
                clickable
                :to="item.to"
                :exact="item.exact"
                active-class="active-menu-class"
                class="app-menu__nested"
                @click="onItem(item)"
              >
                <q-item-section avatar>
                  <q-icon
                    :name="item.icon"
                    size="20px"
                    class="fw-400"
                  />
                </q-item-section>

                <q-item-section>
                  {{ item.label }}
                </q-item-section>
              </q-item>

              <!-- ============================================
                  SETTINGS / NESTED GROUP
                  ============================================ -->
              <!-- <q-expansion-item
                v-else
                dense
                :icon="item.icon"
                :label="item.label"
                :model-value="isOpen(item)"
                header-class="app-menu__group"
                class="app-menu__nested-settings"
                @update:model-value="(v) => setOpen(item.key, v)"
              > -->
              <q-expansion-item
                v-else
                dense
                :icon="item.icon"
                :label="item.label"
                :model-value="isOpen(item)"
                header-class="app-menu__nested-group"
                class="app-menu__nested-settings"
                @update:model-value="(v) => setOpen(item.key, v)"
              >
                <q-item
                  v-for="child in item.items"
                  :key="child.label"
                  v-ripple
                  dense
                  clickable
                  :to="child.to"
                  :exact="child.exact"
                  active-class="active-menu-class"
                  class="app-menu__nested app-menu__settings-child"
                  @click="onItem(child)"
                >
                  <q-item-section avatar>
                    <q-icon
                      :name="child.icon"
                      size="20px"
                    />
                  </q-item-section>

                  <q-item-section>
                    {{ child.label }}
                  </q-item-section>
                </q-item>
              </q-expansion-item>
            </template>

          </q-expansion-item>

        </template>

      </q-list>

    </q-scroll-area>

    <!-- =====================================================
         FIXED FOOTER
         ===================================================== -->
    <div class="app-menu-footer">

      <!-- Settings expansion -->
      <!-- =====================================================
     SETTINGS - NORMAL MODE
     ===================================================== -->
      <template v-if="showSettings && !mini">

        <q-expansion-item
          dense
          icon="o_settings"
          label="Settings"
          :model-value="isOpen(settingsSection)"
          header-class="app-menu__group app-menu__settings-header"
          class="sakolee-app-menu-footer"
          @update:model-value="
            (v) => setOpen(settingsSection.key, v)
          "
        >

          <template
            v-for="item in settingsSection.items"
            :key="item.key || item.label"
          >

            <!-- ================================================
       MASTERS EXPANSION INSIDE SETTINGS FLYOUT
       ================================================ -->
            <q-expansion-item
              v-if="item.items"
              dense
              :icon="item.icon"
              :label="item.label"
              :model-value="isOpen(item)"
              header-class="app-menu__flyout-group"
              class="app-menu__settings-child-group"
              @update:model-value="
                (v) => setOpen(item.key, v)
              "
            >

              <q-item
                v-for="child in item.items"
                :key="child.label"
                v-ripple
                dense
                clickable
                :to="child.to"
                :exact="child.exact"
                active-class="active-menu-class"
                class="app-menu__flyout-child q-pl-lg"
                @click="onItem(child)"
              >

                <q-item-section avatar>
                  <q-icon
                    :name="child.icon"
                    size="20px"
                  />
                </q-item-section>

                <q-item-section>
                  {{ child.label }}
                </q-item-section>

              </q-item>

            </q-expansion-item>

            <!-- ================================================
       NORMAL SETTINGS ITEM
       ================================================ -->
            <q-item
              v-else
              v-ripple
              v-close-popup
              dense
              clickable
              :to="item.to"
              :exact="item.exact"
              active-class="active-menu-class"
              class="app-menu__settings-child"
              @click="onItem(item)"
            >

              <q-item-section avatar>
                <q-icon
                  :name="item.icon"
                  size="20px"
                />
              </q-item-section>

              <q-item-section>
                {{ item.label }}
              </q-item-section>

            </q-item>

          </template>

        </q-expansion-item>

      </template>

      <!-- =================================================
           MINI SETTINGS
           ================================================= -->
      <q-item
        v-else-if="showSettings"
        v-ripple
        dense
        clickable
        class="app-menu__footer-item"
        @mouseenter="openFlyout('settings')"
        @mouseleave="scheduleFlyoutClose"
        @click="openFlyout('settings')"
      >

        <q-item-section avatar>
          <q-icon
            name="o_settings"
            size="22px"
          />
        </q-item-section>

        <q-menu
          :model-value="flyoutKey === 'settings'"
          anchor="bottom right"
          self="bottom left"
          :offset="[0, 0]"
          no-parent-event
          no-focus
          no-refocus
          @update:model-value="
            (open) => (flyoutKey = open ? 'settings' : null)
          "
        >

          <!-- <q-list
            dense
            class="app-menu__flyout"
            @mouseenter="openFlyout('settings')"
            @mouseleave="scheduleFlyoutClose"
          >

            <q-item-label
              header
              class="app-menu__flyout-head"
            >
              Settings
            </q-item-label>

            <q-item
              v-for="item in settingsSection.items"
              :key="item.label"
              v-ripple
              v-close-popup
              dense
              clickable
              :to="item.to"
              :exact="item.exact"
              active-class="active-menu-class"
              @click="onItem(item)"
            >

              <q-item-section avatar>
                <q-icon
                  :name="item.icon"
                  size="20px"
                />
              </q-item-section>

              <q-item-section>
                {{ item.label }}
              </q-item-section>

            </q-item>

          </q-list> -->
          <q-list
            dense
            class="app-menu__flyout"
            @mouseenter="openFlyout('settings')"
            @mouseleave="scheduleFlyoutClose"
          >
            <q-item-label
              header
              class="app-menu__flyout-head"
            >
              Settings
            </q-item-label>

            <template
              v-for="item in settingsSection.items"
              :key="item.key || item.label"
            >

              <!-- ============================================
         SETTINGS ITEM WITH CHILDREN
         ============================================ -->
              <q-item
                v-if="item.items"
                v-ripple
                dense
                clickable
                class="app-menu__flyout-parent"
                @mouseenter="openNestedFlyout('settings-' + item.key)"
                @mouseleave="scheduleNestedFlyoutClose"
                @click="openNestedFlyout('settings-' + item.key)"
              >

                <q-item-section avatar>
                  <q-icon
                    :name="item.icon"
                    size="20px"
                  />
                </q-item-section>

                <q-item-section>
                  {{ item.label }}
                </q-item-section>

                <q-item-section side>
                  <q-icon
                    name="o_chevron_right"
                    size="18px"
                  />
                </q-item-section>

                <!-- ==========================================
           NESTED MASTERS MENU
           ========================================== -->
                <q-menu
                  :model-value="nestedFlyoutKey === 'settings-' + item.key"
                  anchor="top right"
                  self="top left"
                  :offset="[0, 0]"
                  no-parent-event
                  no-focus
                  no-refocus
                  transition-show="jump-right"
                  transition-hide="jump-left"
                  @update:model-value="
                    (open) =>
                      nestedFlyoutKey =
                      open ? 'settings-' + item.key : null
                  "
                >

                  <q-list
                    dense
                    class="app-menu__nested-flyout"
                    @mouseenter="openNestedFlyout('settings-' + item.key)"
                    @mouseleave="scheduleNestedFlyoutClose"
                  >

                    <q-item-label
                      header
                      class="app-menu__flyout-head"
                    >
                      {{ item.label }}
                    </q-item-label>

                    <q-item
                      v-for="child in item.items"
                      :key="child.label"
                      v-ripple
                      v-close-popup
                      dense
                      clickable
                      :to="child.to"
                      :exact="child.exact"
                      active-class="active-menu-class"
                      @click="onItem(child)"
                    >

                      <q-item-section avatar>
                        <q-icon
                          :name="child.icon"
                          size="20px"
                        />
                      </q-item-section>

                      <q-item-section>
                        {{ child.label }}
                      </q-item-section>

                    </q-item>

                  </q-list>

                </q-menu>

              </q-item>

              <!-- ============================================
         NORMAL SETTINGS ITEM
         ============================================ -->
              <q-item
                v-else
                v-ripple
                v-close-popup
                dense
                clickable
                :to="item.to"
                :exact="item.exact"
                active-class="active-menu-class"
                @click="onItem(item)"
              >

                <q-item-section avatar>
                  <q-icon
                    :name="item.icon"
                    size="20px"
                  />
                </q-item-section>

                <q-item-section>
                  {{ item.label }}
                </q-item-section>

              </q-item>

            </template>

          </q-list>
        </q-menu>

        <q-tooltip
          v-if="mini"
          anchor="center right"
          self="center left"
        >
          Settings
        </q-tooltip>

      </q-item>

      <!-- =================================================
           LOGOUT
           ================================================= -->
      <q-item
        v-ripple
        dense
        clickable
        class="app-menu__footer-item logout-item" :class="mini ? 'pl-0' : 'pl-15'"
        @click="handleLogout"
      >

        <q-item-section avatar>
          <q-icon
            name="o_logout"
            size="22px"
          />
        </q-item-section>

        <q-item-section
          v-if="!mini"
          class="fw-500"
        >
          Logout
        </q-item-section>

        <q-tooltip
          v-if="mini"
          anchor="center right"
          self="center left"
        >
          Logout
        </q-tooltip>

      </q-item>

    </div>

  </div>
</template>

<script setup>
import { computed, reactive, ref, onBeforeUnmount } from "vue";
import { LocalStorage } from "quasar";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import { Permissions } from "composables/usePermissions";

defineProps({
  // Collapsed to the icon rail: each group offers its children as a flyout beside its icon.
  mini: { type: Boolean, default: false }
});

const authStore = useAuthStore();
const router = useRouter();
const loggedUserRole = computed(() => authStore.user?.tenants[0]?.roleNames[0]);
console.log("Logged User Role:", loggedUserRole.value);

// Which group is showing its children beside the rail. One at a time — they would overlap otherwise.
const flyoutKey = ref(null);
let flyoutTimer = null;

const openFlyout = (key) => {
  clearTimeout(flyoutTimer);
  flyoutKey.value = key;
};

const handleLogout = async () => {
  await authStore.logout();
  router.replace({ name: "login" });
};

// Delayed, so the pointer can cross from the icon into the flyout without it closing underneath.
const scheduleFlyoutClose = () => {
  clearTimeout(flyoutTimer);

  flyoutTimer = setTimeout(() => {
    // Keep Settings flyout open while a nested menu is active
    if (nestedFlyoutKey.value) {
      return;
    }

    flyoutKey.value = null;
  }, 300);
};

// onBeforeUnmount(() => clearTimeout(flyoutTimer));

onBeforeUnmount(() => {
  clearTimeout(flyoutTimer);
  clearTimeout(nestedFlyoutTimer);
});

// Menu items with an `action` (e.g. Logout) run a handler instead of navigating.
const onItem = async (item) => {
  if (item.action === "logout") {
    await authStore.logout();
    router.replace({ name: "login" });
  } else if (item.action === "logoutAll") {
    await authStore.logoutAll();
    router.replace({ name: "login" });
  }
};

const nestedFlyoutKey = ref(null);
let nestedFlyoutTimer = null;

// const openNestedFlyout = (key) => {
//   clearTimeout(nestedFlyoutTimer);
//   clearTimeout(flyoutTimer);

//   // Make sure parent Settings flyout stays open
//   flyoutKey.value = "settings";

//   nestedFlyoutKey.value = key;
// };
const openNestedFlyout = (key) => {
  clearTimeout(nestedFlyoutTimer);
  clearTimeout(flyoutTimer);
  nestedFlyoutKey.value = key;
};

const scheduleNestedFlyoutClose = () => {
  clearTimeout(nestedFlyoutTimer);

  nestedFlyoutTimer = setTimeout(() => {
    nestedFlyoutKey.value = null;
  }, 300);
};

// Ordered by application flow: overview → set up → configure → operate → personal. `permissions:
// null` → visible to every authenticated user.
const sections = [
  {
    key: "overview",
    label: null,
    items: [
      { label: "Dashboard", icon: "o_dashboard", to: "/dashboard", permissions: null }
    ]
  },
  {
    // Family RECORDS — parents and household accounts. Not to be confused with Family Statuses,
    // which is the lookup list of status names that a family record's status field draws from;
    // that lives under Masters.
    key: "families",
    label: "Families",
    icon: "o_family_restroom",
    items: [
      { label: "All Families", icon: "o_groups", to: "/families", permissions: [Permissions.FamiliesRead] },
      // Gated like its route — part of the Families area.
      { label: "Quick Registration", icon: "o_how_to_reg", to: { name: "family_quick_registration" }, permissions: [Permissions.FamiliesRead] },
      // Families Settings
      {
        key: "families-settings",
        label: "Settings",
        icon: "o_settings",
        items: [
          {
            label: "Family Statuses",
            icon: "o_flag",
            to: "/familystatus",
            permissions: [Permissions.FamilyStatusesRead]
          },
          {
            label: "Studio Locations",
            icon: "o_location_on",
            to: "/locations",
            permissions: [Permissions.LocationsRead]
          },
          {
            label: "Family Relations",
            icon: "o_supervised_user_circle",
            to: "/family-relations",
            permissions: [Permissions.FamilyRelationsRead]
          },
          {
            label: "Hear About Us",
            icon: "o_campaign",
            to: "/hear-about-us",
            permissions: [Permissions.HearAboutUsRead]
          },
          {
            label: "Billing Methods",
            icon: "o_account_balance",
            to: "/billing-methods",
            permissions: [Permissions.BillingMethodsRead]
          },
          {
            label: "Membership Types",
            icon: "o_card_membership",
            to: "/membership-types",
            permissions: [Permissions.MembershipTypesRead]
          },
          {
            label: "E-Payment Schedule",
            icon: "o_payments",
            to: "/e-payment-schedule",
            permissions: [Permissions.EPaymentSchedulesRead]
          },
          {
            label: "Account Types",
            icon: "o_account_balance_wallet",
            to: "/account-type",
            permissions: [Permissions.AccountTypesRead]
          },
        ]
      }
      // { label: "Email/Text Families", icon: "o_mail", to: "/families/email", permissions: null },
      // { label: "Drop Unpaid Families", icon: "o_money_off", to: "/families/drop-unpaid", permissions: null },
      // { label: "Lead Files", icon: "o_contact_page", to: "/families/leads", permissions: null },
      // { label: "Family Report", icon: "o_summarize", to: "/families/report", permissions: null }
    ]
  },
  {
    key: "students",
    label: "Students",
    icon: "o_group",
    items: [
      { label: "All Students", icon: "o_person", to: "/students", permissions: [Permissions.StudentsRead] },
      // Students Settings
      {
        key: "students-settings",
        label: "Settings",
        icon: "o_settings",
        items: [
          {
            label: "T-Shirt Sizes",
            icon: "o_checkroom",
            to: "/t-shirt-sizes",
            permissions: [Permissions.TShirtSizesRead]
          },
          {
            label: "Student Grade Levels",
            icon: "o_school",
            to: "/student-grade-levels",
            permissions: [Permissions.StudentGradeLevelsRead]
          }
        ]
      }
    ]
  },
  {
    key: "administration",
    label: "Administration",
    icon: "o_corporate_fare",
    items: [
      { label: "Tenants", icon: "o_apartment", to: "/tenants", permissions: [Permissions.TenantsWrite] },
      { label: "Person", icon: "o_group", to: "/persons", permissions: [Permissions.PersonsRead] },
    ]
  },
  {
    key: "class",
    label: "Classes",
    icon: "o_school",
    items: [
      { label: "All Classes", icon: "o_class", to: "/classes", permissions: [Permissions.ClassesRead] },
      // Classes Settings
      {
        key: "classes-settings",
        label: "Settings",
        icon: "o_settings",
        items: [
          {
            label: "Class Rooms",
            icon: "o_class",
            to: "/class-rooms",
            permissions: [Permissions.ClassRoomsRead]
          },
          {
            label: "Class Sessions",
            icon: "o_date_range",
            to: "/sessions",
            permissions: [Permissions.ClassSessionsRead]
          },
          {
            label: "Class Categories",
            icon: "o_category",
            to: "/class-categories",
            permissions: [Permissions.ClassCategoriesRead]
          },
          {
            label: "Billing Cycles",
            icon: "o_autorenew",
            to: "/billing-cycles",
            permissions: [Permissions.BillingCyclesRead]
          }
        ]
      }
    ]
  },
  // {
  //   // Tenant lookup lists — the value sets that dropdowns on the entity forms are populated from.
  //   // FamilyStatus is one of these: { FamilyStatusId, TenantId, Name }, nothing more, and it backs
  //   // the "Family Status" field on a family record.
  //   key: "masters",
  //   label: "Masters",
  //   icon: "o_list_alt",
  //   items: [
  //     { label: "Family Statuses", icon: "o_flag", to: "/familystatus", permissions: [Permissions.FamilyStatusesRead] },
  //     { label: "Studio Locations", icon: "o_location_on", to: "/locations", permissions: [Permissions.LocationsRead] },
  //     { label: "Class Categories", icon: "o_category", to: "/class-categories", permissions: [Permissions.ClassCategoriesRead] },
  //     { label: "Billing Cycles", icon: "o_autorenew", to: "/billing-cycles", permissions: [Permissions.BillingCyclesRead] },
  //     { label: "Class Sessions", icon: "o_date_range", to: "/sessions", permissions: [Permissions.SessionsRead] },
  //     { label: "Billing Methods", icon: "o_account_balance", to: "/billing-methods", permissions: [Permissions.BillingMethodsRead] },
  //     { label: "Family Relations", icon: "o_supervised_user_circle", to: "/family-relations", permissions: [Permissions.FamilyRelationsRead] }
  //   ]
  // },
  {
    key: "staff",
    label: "Staffs",
    icon: "o_badge",
    items: [
      { label: "Staffs", icon: "o_assignment_ind", to: "/users", permissions: [Permissions.UsersRead] }
    ]
  },
  {
    key: "access-management",
    label: "Access Management",
    icon: "o_lock",
    items: [
      // { label: "Permission Groups", icon: "o_workspaces", to: "/permission-groups", permissions: [Permissions.GroupsManage] },
      { label: "Roles", icon: "o_admin_panel_settings", to: "/roles", permissions: [Permissions.RolesWrite] }
      // { label: "User Groups", icon: "o_groups", to: "/user-groups", permissions: [Permissions.UsersGroupManagement] }
    ]
  },
  // {
  //   // Tenant-wide settings and universal features (email, and future cross-cutting settings).
  //   key: "settings",
  //   label: "Tenant Settings",
  //   icon: "o_settings",
  //   items: [
  //     { label: "Email Accounts", icon: "o_mail", to: "/smtp-accounts", permissions: [Permissions.EmailManage] },
  //     { label: "Email Templates", icon: "o_drafts", to: "/email-templates", permissions: [Permissions.EmailManage] },
  //     // Gated on MANAGE, not read: working roles hold optionSets.read so their dropdowns resolve, but the
  //     // lists are configuration and only Super Admin / Tenant Admin maintain them.
  //     { label: "Option Sets", icon: "o_list_alt", to: "/option-sets", permissions: [Permissions.OptionSetsManage] },
  //     { label: "Tag Management", icon: "o_label", to: "/settings/tags", permissions: [Permissions.SettingsManage] },
  //     { label: "Sticky Notes", icon: "o_sticky_note_2", to: "/settings/sticky-notes", permissions: [Permissions.SettingsManage] },
  //     { label: "Modified Log", icon: "o_manage_history", to: "/settings/modified-log-config", permissions: [Permissions.SettingsManage] },
  //     { label: "Deleted Records", icon: "o_restore_from_trash", to: "/settings/retention", permissions: [Permissions.RecordsAdminDelete] }
  //   ]
  // },

  // Commented as moved all within Setting Menu--------------------------------------------------------------------
  // {
  //   key: "account",
  //   label: "Account",
  //   icon: "o_account_circle",
  //   items: [
  //     { label: "My Account", icon: "o_manage_accounts", to: "/account", permissions: null },
  //     { label: "Profile", icon: "o_person", to: { name: "profile" }, permissions: null },
  //     { label: "Change Password", icon: "o_lock", to: { name: "change_password" }, permissions: null },
  //     // The full list behind the bell's "View all". Named and ordered as the avatar menu has it, since
  //     // both lead to the same four pages and reading differently in each is what makes one look missing.
  //     // { label: "My Notifications", icon: "o_notifications", to: { name: "uf_notifications" }, permissions: null },
  //     // { label: "My Mentions", icon: "o_alternate_email", to: { name: "uf_mentions" }, permissions: null },
  //     // { label: "My Pinned", icon: "o_push_pin", to: { name: "uf_pinned" }, permissions: null },
  //     // { label: "Notification Preferences", icon: "o_tune", to: { name: "uf_notification_preferences" }, permissions: null },
  //     // { label: "Logout", icon: "o_logout", action: "logout", permissions: null },
  //     // { label: "Logout all devices", icon: "o_devices", action: "logoutAll", permissions: null }
  //   ]
  // }
];

const settingsMenu = {
  key: "settings",
  label: "Settings",
  icon: "o_settings",
  items: [
    // {
    //   key: "settings-masters",
    //   label: "Masters",
    //   icon: "o_list_alt",
    //   items: [
    //     {
    //       label: "Family Statuses",
    //       icon: "o_flag",
    //       to: "/familystatus",
    //       permissions: [Permissions.FamilyStatusesRead]
    //     },
    //     {
    //       label: "Studio Locations",
    //       icon: "o_location_on",
    //       to: "/locations",
    //       permissions: [Permissions.LocationsRead]
    //     },
    //     {
    //       label: "Class Rooms",
    //       icon: "o_class",
    //       to: "/class-rooms",
    //       permissions: [Permissions.ClassRoomsRead]
    //     },
    //     {
    //       label: "Class Sessions",
    //       icon: "o_date_range",
    //       to: "/sessions",
    //       permissions: [Permissions.ClassSessionsRead]
    //     },
    //     {
    //       label: "Billing Methods",
    //       icon: "o_account_balance",
    //       to: "/billing-methods",
    //       permissions: [Permissions.BillingMethodsRead]
    //     },
    //     {
    //       label: "Membership Types",
    //       icon: "o_card_membership",
    //       to: "/membership-types",
    //       permissions: [Permissions.MembershipTypesRead]
    //     },
    //     {
    //       label: "Student Grade Levels",
    //       icon: "o_school",
    //       to: "/student-grade-levels",
    //       permissions: [Permissions.StudentGradeLevelsRead]
    //     },

    //     {
    //       label: "Family Relations",
    //       icon: "o_supervised_user_circle",
    //       to: "/family-relations",
    //       permissions: [Permissions.FamilyRelationsRead]
    //     },
    //     {
    //       label: "Class Categories",
    //       icon: "o_category",
    //       to: "/class-categories",
    //       permissions: [Permissions.ClassCategoriesRead]
    //     },
    //     {
    //       label: "T-Shirt Sizes",
    //       icon: "o_checkroom",
    //       to: "/t-shirt-sizes",
    //       permissions: [Permissions.TShirtSizesRead]
    //     },
    //     {
    //       label: "Billing Cycles",
    //       icon: "o_autorenew",
    //       to: "/billing-cycles",
    //       permissions: [Permissions.BillingCyclesRead]
    //     },
    //     {
    //       label: "Hear About Us",
    //       icon: "o_campaign",
    //       to: "/hear-about-us",
    //       permissions: [Permissions.HearAboutUsRead]
    //     },
    //     {
    //       label: "E-Payment Schedule",
    //       icon: "o_payments",
    //       to: "/e-payment-schedule",
    //       permissions: [Permissions.EPaymentSchedulesRead]
    //     },
    //     {
    //       label: "Account Types",
    //       icon: "o_account_balance_wallet",
    //       to: "/account-type",
    //       permissions: [Permissions.AccountTypesRead]
    //     },
    //   ]
    // },
    {
      label: "Policies",
      icon: "o_policy",
      to: "/policies",
      permissions: [Permissions.PoliciesRead]
    },
    {
      label: "My Account",
      icon: "o_manage_accounts",
      to: "/account",
      permissions: null
    },
    {
      label: "Profile",
      icon: "o_person",
      to: { name: "profile" },
      permissions: null
    },
    {
      label: "Change Password",
      icon: "o_lock",
      to: { name: "change_password" },
      permissions: null
    }
  ]
};

const canSee = (permissions) => !permissions || authStore.hasAnyPermission(permissions);
console.log("Can See Permissionis:", canSee());

// Settings is gated by its permission-bound entries (Masters). The personal pages it also carries
// (My Account, Profile, Change Password) stay reachable from the avatar menu, so a role holding none
// of the Settings permissions — e.g. Student — doesn't get the Settings menu at all.
const filterItems = (items) =>
  items
    .map((item) => (item.items ? { ...item, items: filterItems(item.items) } : item))
    .filter((item) => (item.items ? item.items.length : canSee(item.permissions)));

const settingsSection = computed(() => ({ ...settingsMenu, items: filterItems(settingsMenu.items) }));

// const showSettings = computed(() =>
//   settingsSection.value.items.some((item) => item.items || item.permissions));
const showSettings = computed(() =>
  settingsSection.value.items.length > 0
);
// const visibleSections = computed(() =>
//   sections
//     .map((section) => ({ ...section, items: section.items.filter((item) => canSee(item.permissions)) }))
//     .filter((section) => section.items.length));
// The above is a simple filter, but it doesn't handle nested items. The below does, and also filters out
// any section that has no visible items at all.
const filterSectionItems = (items) => {
  return items
    .map((item) => {
      if (item.items) {
        return {
          ...item,
          items: filterSectionItems(item.items)
        };
      }
      return item;
    })
    .filter((item) => {
      if (item.items) {
        return item.items.length > 0;
      }
      return canSee(item.permissions);
    });
};
const visibleSections = computed(() =>
  sections
    .map((section) => ({
      ...section,
      items: filterSectionItems(section.items)
    }))
    .filter((section) => section.items.length > 0)
);

// Per-group collapse state, persisted to LocalStorage so the user's expand/collapse choices survive a
// page refresh. The stored object holds only the collapsed groups ({ [sectionKey]: true }).
const STORAGE_KEY = "appMenuCollapsed";
const collapsed = reactive(LocalStorage.getItem(STORAGE_KEY) || {});
const isOpen = (section) => collapsed[section.key] !== true;
const setOpen = (key, open) => {
  if (open) {
    delete collapsed[key];
  } else {
    collapsed[key] = true;
  }
  LocalStorage.set(STORAGE_KEY, { ...collapsed });
};
</script>

<style scoped lang="scss">

/* ============================================================
   MENU COLORS
   ============================================================ */

$menu-color: #464554;
$parent-menu-color: #131b2e;
$parent-icon-color: #464554;
$expand-icon-color: #767586;

/* ============================================================
   ACTIVE MENU
   ============================================================ */

.active-menu-class {
  color: var(--primary) !important;
}

/* Active label */
:deep(.active-menu-class .q-item__section:not(.q-item__section--avatar)) {
  color: var(--primary) !important;
}

/* Active icon */
:deep(.active-menu-class .q-item__section--avatar .q-icon) {
  color: var(--primary) !important;
}

/* ============================================================
   GENERAL MENU
   ============================================================ */

.app-menu :deep(.q-item) {
  min-height: 40px;
}

.app-menu :deep(.q-item__section--avatar) {
  min-width: 32px;
  padding-right: 8px;
}

/* ============================================================
   DASHBOARD / UNGROUPED PARENT
   ============================================================ */

.dashboard-menu {
  padding: 4px 12px !important;
}

.dashboard-menu :deep(
  .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 14px;
  font-weight: 500;
  color: $parent-menu-color;
}

.dashboard-menu :deep(
  .q-item__section--avatar .q-icon
) {
  font-size: 22px !important;
  color: $parent-icon-color;
}

/* ============================================================
   MAIN PARENT EXPANSION
   Families / Students / Classes / etc.
   ============================================================ */

.app-menu :deep(.app-menu__group) {
  min-height: 40px;
  padding: 4px 12px;
}

/* Parent label */
.app-menu :deep(
  .app-menu__group .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 14px;
  font-weight: 500;
  color: $parent-menu-color;
}

/* Parent icon */
.app-menu :deep(
  .app-menu__group .q-item__section--avatar .q-icon
) {
  font-size: 22px !important;
  color: $parent-icon-color;
}

/* Parent expand arrow */
.app-menu :deep(
  .app-menu__group .q-expansion-item__toggle-icon
) {
  font-size: 18px !important;
  color: $expand-icon-color;
}

/* ============================================================
   NORMAL CHILD
   All Families / All Students / etc.
   ============================================================ */

.app-menu__nested {
  padding-left: 20px;
}

/* Child label */
:deep(
  .app-menu__nested .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Child icon */
:deep(
  .app-menu__nested .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* ============================================================
   NESTED EXPANSION
   Families → Settings
   Students → Settings
   Classes → Settings
   ============================================================ */

.app-menu :deep(.app-menu__nested-group) {
  min-height: 40px;
  padding: 4px 16px 4px 20px;
}

/* Nested parent label */
.app-menu :deep(
  .app-menu__nested-group .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Nested parent icon */
.app-menu :deep(
  .app-menu__nested-group .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* Nested parent arrow */
.app-menu :deep(
  .app-menu__nested-group .q-expansion-item__toggle-icon
) {
  font-size: 18px !important;
  color: $expand-icon-color;
}

/* ============================================================
   NESTED CHILD
   Family Statuses / Studio Locations / etc.
   ============================================================ */

.app-menu__settings-child {
  padding-left: 20px !important;
}

/* ============================================================
   FLYOUT
   ============================================================ */

.app-menu__flyout {
  min-width: 208px;
  padding: 2px 0 4px;
}

.app-menu__flyout :deep(.q-item) {
  min-height: 34px;
}

.app-menu__flyout :deep(.q-item__section--avatar) {
  min-width: 32px;
  padding-right: 8px;
}

/* Flyout heading */
.app-menu__flyout-head {
  min-height: auto;
  padding: 8px 16px 4px;

  font-size: 14px;
  font-weight: 600;

  color: $menu-color;
}

/* ============================================================
   MINI FLYOUT PARENT
   Example: Settings → Masters
   ============================================================ */

.app-menu__flyout-parent {
  min-height: 36px;
  padding: 4px 12px;

  border-radius: 6px;
}

/* Parent icon */
.app-menu__flyout-parent :deep(
  .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* Parent label */
.app-menu__flyout-parent :deep(
  .q-item__section:not(.q-item__section--avatar):not(.q-item__section--side)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Parent arrow */
.app-menu__flyout-parent :deep(
  .q-item__section--side .q-icon
) {
  font-size: 18px !important;
  color: $expand-icon-color;
}

.app-menu__flyout-parent:hover {
  background: #f5f6ff;
}

/* ============================================================
   NESTED FLYOUT
   ============================================================ */

.app-menu__nested-flyout {
  min-width: 230px;
  padding: 2px 0 4px;
}

.app-menu__nested-flyout :deep(.q-item) {
  min-height: 34px;
}

.app-menu__nested-flyout :deep(
  .q-item__section--avatar
) {
  min-width: 32px;
  padding-right: 8px;
}

/* Nested flyout child label */
.app-menu__nested-flyout :deep(
  .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Nested flyout child icon */
.app-menu__nested-flyout :deep(
  .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* ============================================================
   FOOTER
   ============================================================ */

.app-menu-footer {
  flex: 0 0 auto;

  border-top: 1px solid #e5e7eb;

  background: #ffffff;

  padding: 8px 14px;
}

/* ============================================================
   FOOTER SETTINGS PARENT
   ============================================================ */

.app-menu-footer :deep(.app-menu__settings-header) {
  min-height: 40px;
  padding: 4px 12px;
}

/* Settings parent label */
.app-menu-footer :deep(
  .app-menu__settings-header .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 14px;
  font-weight: 500;
  color: $parent-menu-color;
}

/* Settings parent icon */
.app-menu-footer :deep(
  .app-menu__settings-header .q-item__section--avatar .q-icon
) {
  font-size: 22px !important;
  color: $parent-icon-color;
}

/* Settings parent arrow */
.app-menu-footer :deep(
  .app-menu__settings-header .q-expansion-item__toggle-icon
) {
  font-size: 18px !important;
  color: $expand-icon-color;
}

/* ============================================================
   FOOTER SETTINGS CHILDREN
   Policies / My Account / Profile / Change Password
   ============================================================ */

.app-menu-footer :deep(.app-menu__settings-child) {
  min-height: 40px;
  padding: 4px 12px 4px 28px;
}

/* Child label */
.app-menu-footer :deep(
  .app-menu__settings-child .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Child icon */
.app-menu-footer :deep(
  .app-menu__settings-child .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* ============================================================
   FOOTER FLYOUT GROUP
   Settings → Masters
   ============================================================ */

.app-menu-footer :deep(.app-menu__flyout-group) {
  min-height: 36px;
  padding: 4px 12px;
}

/* Group label */
.app-menu-footer :deep(
  .app-menu__flyout-group .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Group icon */
.app-menu-footer :deep(
  .app-menu__flyout-group .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* Group arrow */
.app-menu-footer :deep(
  .app-menu__flyout-group .q-expansion-item__toggle-icon
) {
  font-size: 18px !important;
  color: $expand-icon-color;
}

/* ============================================================
   FOOTER FLYOUT CHILD
   ============================================================ */

.app-menu__flyout-child {
  min-height: 34px;
}

/* Child label */
.app-menu__flyout-child :deep(
  .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 12px;
  font-weight: 400;
  color: $menu-color;
}

/* Child icon */
.app-menu__flyout-child :deep(
  .q-item__section--avatar .q-icon
) {
  font-size: 20px !important;
  color: $menu-color;
}

/* ============================================================
   FOOTER NORMAL ITEM
   ============================================================ */

.app-menu__footer-item {
  min-height: 40px;
  padding: 4px 4px;

  color: $menu-color;
  border-radius: 8px;
}

.app-menu__footer-item:hover {
  background: #f5f6ff;
}

/* ============================================================
   LOGOUT
   ============================================================ */

.logout-item {
  min-height: 40px;
  color: #dc2626 !important;
}

.logout-item :deep(
  .q-item__section--avatar .q-icon
) {
  font-size: 22px !important;
  color: #dc2626 !important;
}

.logout-item :deep(
  .q-item__section:not(.q-item__section--avatar)
) {
  font-size: 14px;
  font-weight: 500;
  color: #dc2626 !important;
}

/* ============================================================
   MENU CONTAINER
   ============================================================ */

.app-menu-wrapper {
  height: 100%;

  display: flex;
  flex-direction: column;

  min-height: 0;
}

.app-menu-scroll {
  flex: 1 1 auto;
  min-height: 0;

  padding: 0 14px;
}

.app-menu {
  padding-bottom: 12px;
}

/* ============================================================
   SETTINGS
   ============================================================ */

.app-menu__settings {
  border-radius: 8px;
}

.app-menu__settings-nested {
  padding-left: 8px;
}

/* ============================================================
   UTILITY
   ============================================================ */

.pl-15 {
  padding-left: 15px !important;
}

.pl-0 {
  padding-left: 0 !important;
}

</style>
