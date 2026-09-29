export default [
  {
    path: "/class-rooms",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "class-rooms",
        component: () => import("modules/class-room/pages/index.vue"),
        meta: { 
          requiresAuth: true, 
          permissions: ["classRooms.read"], 
          title: "Class Rooms" 
        }
      },
      // {
      //   path: "create",
      //   name: "class_room_create",
      //   component: () => import("modules/class-room/components/create_edit.vue"),
      //   meta: { 
      //     requiresAuth: true, 
      //     permissions: ["classRooms.write"], 
      //     title: "Create Class Room" 
      //   }
      // },
      // {
      //   path: ":id/edit",
      //   name: "class_room_edit",
      //   component: () => import("modules/class-room/components/create_edit.vue"),
      //   meta: { 
      //     requiresAuth: true, 
      //     permissions: ["classRooms.write"], 
      //     title: "Edit Class Room" 
      //   }
      // },
      // {
      //   path: ":id",
      //   name: "class_room_detail",
      //   component: () => import("modules/class-room/components/view.vue"),
      //   meta: { 
      //     requiresAuth: true, 
      //     permissions: ["classRooms.read"], 
      //     title: "Class Room Details" 
      //   }
      // }
    ]
  }
];