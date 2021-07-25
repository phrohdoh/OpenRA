#_
(ns openra-map-script)

#_
(defn ns-publics-list
  ([] (ns-publics-list *ns*))
  ([ns] (#(list (ns-name %) (map first (ns-publics %))) ns)))

(defn chat [text & {:keys [prefix prefix-color text-color] :or {prefix nil prefix-color nil text-color nil}}]
   (OpenRA.TextNotificationsManager/AddChatLine prefix text prefix-color text-color))

(defn on-world-loaded [& args]
  (prn "world loaded y'all!")
  (prn '*ns*= *ns* 'args= args)
  (chat "hello from map.clj!") ; doesn't seem to work, too early? ui not loaded?
)

(defn tick [& args] nil)
