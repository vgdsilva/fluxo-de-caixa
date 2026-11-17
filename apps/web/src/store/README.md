# store

Stores do Pinia: estado global compartilhado entre telas, como um serviço singleton com estado. A primeira será a do usuário logado (`auth.store.ts`, fatia 4). Estado que só uma tela usa fica num hook (`hooks/`) ou num context (`context/`).
