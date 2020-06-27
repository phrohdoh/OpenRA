(module $map
    ;; (import "env" "log" (func $log_str (param $ptr i32) (param $len i32)))
    (import "some_other_module" "some_fn" (func $_fn_x (param $x i32) (param $y i32) (result i32)))
    (func $add_3 (param $x i32) (result i32)
        (return (i32.add (local.get $x) (i32.const 3)))
    )
    (func $world_did_load
        ;; todo
    )
    (export "world_did_load" (func $world_did_load))
    (export "add_3" (func $add_3))
)
