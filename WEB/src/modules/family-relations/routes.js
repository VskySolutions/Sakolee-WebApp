export default [
  {
    path: "/family-relations",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all family relations (Index Page)
      {
        path: "",
        name: "/family-relations",
        component: () => import("modules/family-relations/pages/index.vue"),
        meta: { requiresAuth: true, title: "Family Relations" }
      },
      // Route for creating a new family relation record
      {
        path: "create",
        name: "family_relation_create",
        component: () => import("modules/family-relations/components/create_edit.vue"),
        meta: { requiresAuth: true, title: "Create Family Relation" }
      },
      // // Route for editing an existing family relation record by its unique identifier
      // {
      //   path: ":id/edit",
      //   name: "family_relation_edit",
      //   component: () => import("modules/family-relations/components/create_edit.vue"),
      //   meta: { requiresAuth: true, title: "Edit Family Relation" }
      // },
      // Route for viewing detailed information of a specific family relation record
      {
        path: ":id",
        name: "family_relation_detail",
        component: () => import("modules/family-relations/components/view_realtion.vue"),
        meta: { requiresAuth: true, title: "Family Relation Details" }
      }
    ]
  }
];