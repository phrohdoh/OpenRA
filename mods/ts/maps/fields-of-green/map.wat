(module $map
    (import "env" "mem" (memory 1))
    (import "env" "log" (func $log_str (param $ptr i32) (param $len i32)))
    ;; (import "some_other_module" "some_fn" (func $_fn_x (param $x i32) (param $y i32) (result i32)))
    (func $add_3 (param $x i32) (result i32)
        (return (i32.add (local.get $x) (i32.const 3)))
    )
    (func $world_did_load
        ;; log, in the host env, "hello"
        (i32.store8 (i32.const 0x0) (i32.const 0x68)) ;; h
        (i32.store8 (i32.const 0x1) (i32.const 0x65)) ;; e
        (i32.store8 (i32.const 0x2) (i32.const 0x6c)) ;; l
        (i32.store8 (i32.const 0x3) (i32.const 0x6c)) ;; l
        (i32.store8 (i32.const 0x4) (i32.const 0x6f)) ;; o
        (call $log_str (i32.const 0x0) (i32.const 5))
    )
    (export "world_did_load" (func $world_did_load))
    (export "add_3" (func $add_3))
)
