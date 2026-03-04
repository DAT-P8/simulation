from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class TDFDroneState(_message.Message):
    __slots__ = ("id", "x", "y", "z", "x_v", "y_v", "z_v", "destroyed", "is_evader")
    ID_FIELD_NUMBER: _ClassVar[int]
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    Z_FIELD_NUMBER: _ClassVar[int]
    X_V_FIELD_NUMBER: _ClassVar[int]
    Y_V_FIELD_NUMBER: _ClassVar[int]
    Z_V_FIELD_NUMBER: _ClassVar[int]
    DESTROYED_FIELD_NUMBER: _ClassVar[int]
    IS_EVADER_FIELD_NUMBER: _ClassVar[int]
    id: int
    x: float
    y: float
    z: float
    x_v: float
    y_v: float
    z_v: float
    destroyed: bool
    is_evader: bool
    def __init__(self, id: _Optional[int] = ..., x: _Optional[float] = ..., y: _Optional[float] = ..., z: _Optional[float] = ..., x_v: _Optional[float] = ..., y_v: _Optional[float] = ..., z_v: _Optional[float] = ..., destroyed: bool = ..., is_evader: bool = ...) -> None: ...

class TDFState(_message.Message):
    __slots__ = ("terminated", "sim_id", "drone_states")
    TERMINATED_FIELD_NUMBER: _ClassVar[int]
    SIM_ID_FIELD_NUMBER: _ClassVar[int]
    DRONE_STATES_FIELD_NUMBER: _ClassVar[int]
    terminated: bool
    sim_id: int
    drone_states: _containers.RepeatedCompositeFieldContainer[TDFDroneState]
    def __init__(self, terminated: bool = ..., sim_id: _Optional[int] = ..., drone_states: _Optional[_Iterable[_Union[TDFDroneState, _Mapping]]] = ...) -> None: ...

class TDFDroneAction(_message.Message):
    __slots__ = ("id", "x_f", "y_f", "z_f")
    ID_FIELD_NUMBER: _ClassVar[int]
    X_F_FIELD_NUMBER: _ClassVar[int]
    Y_F_FIELD_NUMBER: _ClassVar[int]
    Z_F_FIELD_NUMBER: _ClassVar[int]
    id: int
    x_f: float
    y_f: float
    z_f: float
    def __init__(self, id: _Optional[int] = ..., x_f: _Optional[float] = ..., y_f: _Optional[float] = ..., z_f: _Optional[float] = ...) -> None: ...

class TDFDoStepRequest(_message.Message):
    __slots__ = ("id", "drone_actions")
    ID_FIELD_NUMBER: _ClassVar[int]
    DRONE_ACTIONS_FIELD_NUMBER: _ClassVar[int]
    id: int
    drone_actions: _containers.RepeatedCompositeFieldContainer[TDFDroneAction]
    def __init__(self, id: _Optional[int] = ..., drone_actions: _Optional[_Iterable[_Union[TDFDroneAction, _Mapping]]] = ...) -> None: ...

class TDFDoStepResponse(_message.Message):
    __slots__ = ("state", "error_msg")
    STATE_FIELD_NUMBER: _ClassVar[int]
    ERROR_MSG_FIELD_NUMBER: _ClassVar[int]
    state: TDFState
    error_msg: str
    def __init__(self, state: _Optional[_Union[TDFState, _Mapping]] = ..., error_msg: _Optional[str] = ...) -> None: ...

class TDFResetRequest(_message.Message):
    __slots__ = ("id",)
    ID_FIELD_NUMBER: _ClassVar[int]
    id: int
    def __init__(self, id: _Optional[int] = ...) -> None: ...

class TDFResetResponse(_message.Message):
    __slots__ = ("state", "error_msg")
    STATE_FIELD_NUMBER: _ClassVar[int]
    ERROR_MSG_FIELD_NUMBER: _ClassVar[int]
    state: TDFState
    error_msg: str
    def __init__(self, state: _Optional[_Union[TDFState, _Mapping]] = ..., error_msg: _Optional[str] = ...) -> None: ...

class TDFCloseRequest(_message.Message):
    __slots__ = ("id",)
    ID_FIELD_NUMBER: _ClassVar[int]
    id: int
    def __init__(self, id: _Optional[int] = ...) -> None: ...

class TDFCloseResponse(_message.Message):
    __slots__ = ("error_msg",)
    ERROR_MSG_FIELD_NUMBER: _ClassVar[int]
    error_msg: str
    def __init__(self, error_msg: _Optional[str] = ...) -> None: ...

class TDFNewRequest(_message.Message):
    __slots__ = ()
    def __init__(self) -> None: ...

class TDFNewResponse(_message.Message):
    __slots__ = ("state", "error_msg")
    STATE_FIELD_NUMBER: _ClassVar[int]
    ERROR_MSG_FIELD_NUMBER: _ClassVar[int]
    state: TDFState
    error_msg: str
    def __init__(self, state: _Optional[_Union[TDFState, _Mapping]] = ..., error_msg: _Optional[str] = ...) -> None: ...
