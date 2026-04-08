from google.protobuf.internal import containers as _containers
from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable, Mapping as _Mapping
from typing import ClassVar as _ClassVar, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class Action(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    ACTION_UNKNOWN_UNSPECIFIED: _ClassVar[Action]
    ACTION_NOTHING: _ClassVar[Action]
    ACTION_LEFT: _ClassVar[Action]
    ACTION_LEFT_UP: _ClassVar[Action]
    ACTION_UP: _ClassVar[Action]
    ACTION_RIGHT_UP: _ClassVar[Action]
    ACTION_RIGHT: _ClassVar[Action]
    ACTION_RIGHT_DOWN: _ClassVar[Action]
    ACTION_DOWN: _ClassVar[Action]
    ACTION_LEFT_DOWN: _ClassVar[Action]
ACTION_UNKNOWN_UNSPECIFIED: Action
ACTION_NOTHING: Action
ACTION_LEFT: Action
ACTION_LEFT_UP: Action
ACTION_UP: Action
ACTION_RIGHT_UP: Action
ACTION_RIGHT: Action
ACTION_RIGHT_DOWN: Action
ACTION_DOWN: Action
ACTION_LEFT_DOWN: Action

class DroneState(_message.Message):
    __slots__ = ("id", "x", "y", "destroyed", "is_evader")
    ID_FIELD_NUMBER: _ClassVar[int]
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    DESTROYED_FIELD_NUMBER: _ClassVar[int]
    IS_EVADER_FIELD_NUMBER: _ClassVar[int]
    id: int
    x: int
    y: int
    destroyed: bool
    is_evader: bool
    def __init__(self, id: _Optional[int] = ..., x: _Optional[int] = ..., y: _Optional[int] = ..., destroyed: bool = ..., is_evader: bool = ...) -> None: ...

class State(_message.Message):
    __slots__ = ("sim_id", "terminated", "drone_states", "events")
    SIM_ID_FIELD_NUMBER: _ClassVar[int]
    TERMINATED_FIELD_NUMBER: _ClassVar[int]
    DRONE_STATES_FIELD_NUMBER: _ClassVar[int]
    EVENTS_FIELD_NUMBER: _ClassVar[int]
    sim_id: int
    terminated: bool
    drone_states: _containers.RepeatedCompositeFieldContainer[DroneState]
    events: _containers.RepeatedCompositeFieldContainer[Event]
    def __init__(self, sim_id: _Optional[int] = ..., terminated: bool = ..., drone_states: _Optional[_Iterable[_Union[DroneState, _Mapping]]] = ..., events: _Optional[_Iterable[_Union[Event, _Mapping]]] = ...) -> None: ...

class Event(_message.Message):
    __slots__ = ("collision_event", "target_reached_event", "out_of_bounds_event")
    COLLISION_EVENT_FIELD_NUMBER: _ClassVar[int]
    TARGET_REACHED_EVENT_FIELD_NUMBER: _ClassVar[int]
    OUT_OF_BOUNDS_EVENT_FIELD_NUMBER: _ClassVar[int]
    collision_event: CollisionEvent
    target_reached_event: TargetReachedEvent
    out_of_bounds_event: OutOfBoundsEvent
    def __init__(self, collision_event: _Optional[_Union[CollisionEvent, _Mapping]] = ..., target_reached_event: _Optional[_Union[TargetReachedEvent, _Mapping]] = ..., out_of_bounds_event: _Optional[_Union[OutOfBoundsEvent, _Mapping]] = ...) -> None: ...

class CollisionEvent(_message.Message):
    __slots__ = ("drone_ids",)
    DRONE_IDS_FIELD_NUMBER: _ClassVar[int]
    drone_ids: _containers.RepeatedScalarFieldContainer[int]
    def __init__(self, drone_ids: _Optional[_Iterable[int]] = ...) -> None: ...

class TargetReachedEvent(_message.Message):
    __slots__ = ("drone_ids",)
    DRONE_IDS_FIELD_NUMBER: _ClassVar[int]
    drone_ids: _containers.RepeatedScalarFieldContainer[int]
    def __init__(self, drone_ids: _Optional[_Iterable[int]] = ...) -> None: ...

class OutOfBoundsEvent(_message.Message):
    __slots__ = ("drone_ids",)
    DRONE_IDS_FIELD_NUMBER: _ClassVar[int]
    drone_ids: _containers.RepeatedScalarFieldContainer[int]
    def __init__(self, drone_ids: _Optional[_Iterable[int]] = ...) -> None: ...

class MapSpec(_message.Message):
    __slots__ = ("square_map",)
    SQUARE_MAP_FIELD_NUMBER: _ClassVar[int]
    square_map: SquareMap
    def __init__(self, square_map: _Optional[_Union[SquareMap, _Mapping]] = ...) -> None: ...

class SquareMap(_message.Message):
    __slots__ = ("width", "height", "target_x", "target_y")
    WIDTH_FIELD_NUMBER: _ClassVar[int]
    HEIGHT_FIELD_NUMBER: _ClassVar[int]
    TARGET_X_FIELD_NUMBER: _ClassVar[int]
    TARGET_Y_FIELD_NUMBER: _ClassVar[int]
    width: int
    height: int
    target_x: int
    target_y: int
    def __init__(self, width: _Optional[int] = ..., height: _Optional[int] = ..., target_x: _Optional[int] = ..., target_y: _Optional[int] = ...) -> None: ...

class DroneAction(_message.Message):
    __slots__ = ("id", "action", "velocity")
    ID_FIELD_NUMBER: _ClassVar[int]
    ACTION_FIELD_NUMBER: _ClassVar[int]
    VELOCITY_FIELD_NUMBER: _ClassVar[int]
    id: int
    action: Action
    velocity: int
    def __init__(self, id: _Optional[int] = ..., action: _Optional[_Union[Action, str]] = ..., velocity: _Optional[int] = ...) -> None: ...

class StateResponse(_message.Message):
    __slots__ = ("state", "error_message")
    STATE_FIELD_NUMBER: _ClassVar[int]
    ERROR_MESSAGE_FIELD_NUMBER: _ClassVar[int]
    state: State
    error_message: str
    def __init__(self, state: _Optional[_Union[State, _Mapping]] = ..., error_message: _Optional[str] = ...) -> None: ...

class DoStepRequest(_message.Message):
    __slots__ = ("sim_id", "drone_actions")
    SIM_ID_FIELD_NUMBER: _ClassVar[int]
    DRONE_ACTIONS_FIELD_NUMBER: _ClassVar[int]
    sim_id: int
    drone_actions: _containers.RepeatedCompositeFieldContainer[DroneAction]
    def __init__(self, sim_id: _Optional[int] = ..., drone_actions: _Optional[_Iterable[_Union[DroneAction, _Mapping]]] = ...) -> None: ...

class DoStepResponse(_message.Message):
    __slots__ = ("state_response",)
    STATE_RESPONSE_FIELD_NUMBER: _ClassVar[int]
    state_response: StateResponse
    def __init__(self, state_response: _Optional[_Union[StateResponse, _Mapping]] = ...) -> None: ...

class ResetRequest(_message.Message):
    __slots__ = ("sim_id",)
    SIM_ID_FIELD_NUMBER: _ClassVar[int]
    sim_id: int
    def __init__(self, sim_id: _Optional[int] = ...) -> None: ...

class ResetResponse(_message.Message):
    __slots__ = ("state_response",)
    STATE_RESPONSE_FIELD_NUMBER: _ClassVar[int]
    state_response: StateResponse
    def __init__(self, state_response: _Optional[_Union[StateResponse, _Mapping]] = ...) -> None: ...

class CloseRequest(_message.Message):
    __slots__ = ("sim_id",)
    SIM_ID_FIELD_NUMBER: _ClassVar[int]
    sim_id: int
    def __init__(self, sim_id: _Optional[int] = ...) -> None: ...

class CloseResponse(_message.Message):
    __slots__ = ("error_message",)
    ERROR_MESSAGE_FIELD_NUMBER: _ClassVar[int]
    error_message: str
    def __init__(self, error_message: _Optional[str] = ...) -> None: ...

class NewRequest(_message.Message):
    __slots__ = ("map", "evader_count", "pursuer_count", "drone_velocity")
    MAP_FIELD_NUMBER: _ClassVar[int]
    EVADER_COUNT_FIELD_NUMBER: _ClassVar[int]
    PURSUER_COUNT_FIELD_NUMBER: _ClassVar[int]
    DRONE_VELOCITY_FIELD_NUMBER: _ClassVar[int]
    map: MapSpec
    evader_count: int
    pursuer_count: int
    drone_velocity: int
    def __init__(self, map: _Optional[_Union[MapSpec, _Mapping]] = ..., evader_count: _Optional[int] = ..., pursuer_count: _Optional[int] = ..., drone_velocity: _Optional[int] = ...) -> None: ...

class NewResponse(_message.Message):
    __slots__ = ("state_response",)
    STATE_RESPONSE_FIELD_NUMBER: _ClassVar[int]
    state_response: StateResponse
    def __init__(self, state_response: _Optional[_Union[StateResponse, _Mapping]] = ...) -> None: ...
