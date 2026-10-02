// export default [
//   {
//     path: "/locations",
//     component: () => import("layouts/layout.vue"),
//     children: [
//       {
//         path: "",
//         name: "locations",
//         component: () => import("modules/location/pages/index.vue"),
//         meta: { requiresAuth: true, permissions: ["locations.read"], title: "Locations" }
//       }
//     ]
//   }
// ];

export default [
  {
    path: "/locations",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all locations (Index Page)
      {
        path: "",
        name: "locations",
        component: () => import("modules/location/pages/index.vue"),
        meta: { requiresAuth: true, title: "Locations" }
      },
      // Route for creating a new location record
      // {
      //   path: "create",
      //   name: "location_create",
      //   component: () => import("modules/location/components/create_edit.vue"),
      //   meta: { requiresAuth: true, title: "Create Location" }
      // },
      // // Route for viewing detailed information of a specific location record
      // {
      //   path: ":id",
      //   name: "location_detail",
      //   component: () => import("modules/location/components/view.vue"),
      //   meta: { requiresAuth: true, title: "Location Details" }
      // }
    ]
  }
];
