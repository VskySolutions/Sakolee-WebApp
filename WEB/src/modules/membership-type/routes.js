export default [
  {
    path: "/membership-types",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all membership types (Index Page)
      {
        path: "",
        name: "membership_types",
        component: () => import("modules/membership-type/pages/index.vue"),
        meta: { requiresAuth: true, title: "Membership Types" }
      },
      // Route for creating a new membership type record
      // {
      //   path: "create",
      //   name: "membership_type_create",
      //   component: () => import("modules/membership-type/components/create_edit.vue"),
      //   meta: { requiresAuth: true, title: "Create Membership Type" }
      // },
      // // Route for viewing detailed information of a specific membership type record
      // {
      //   path: ":id",
      //   name: "membership_type_detail",
      //   component: () => import("modules/membership-type/components/view.vue"),
      //   meta: { requiresAuth: true, title: "Membership Type Details" }
      // }
    ]
  }
];